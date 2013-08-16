using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;

using Microsoft.Build.Tasks;

namespace NRefactoryTestApp
{
#if false
    public class CodeCompletionUtils
    {
        public static IEnumerable<ICompletionData> DoCodeComplete(string editorText, int offset) // not the best way to put in the whole string every time
        {
            var doc = new ReadOnlyDocument(editorText);
            var location = doc.GetLocation(offset);

            string parsedText = editorText; // TODO: Why are there different values in test cases?


            var syntaxTree = new CSharpParser().Parse(parsedText, "program.cs");
            syntaxTree.Freeze();
            var unresolvedFile = syntaxTree.ToTypeSystem();

            var mb = new DefaultCompletionContextProvider(doc, unresolvedFile);

            IProjectContent pctx = new CSharpProjectContent();
            var refs = new List<IUnresolvedAssembly> { mscorlib.Value, systemCore.Value, systemAssembly.Value };
            pctx = pctx.AddAssemblyReferences(refs);
            pctx = pctx.AddOrUpdateFiles(unresolvedFile);

            var cmp = pctx.CreateCompilation();

            var resolver3 = unresolvedFile.GetResolver(cmp, location);
            var engine = new CSharpCompletionEngine(doc, mb, new TestCompletionDataFactory(resolver3), pctx, resolver3.CurrentTypeResolveContext);


            engine.EolMarker = Environment.NewLine;
            engine.FormattingPolicy = FormattingOptionsFactory.CreateMono();

            var data = engine.GetCompletionData(offset, controlSpace: false);
            return data;

        }


        static readonly Lazy<IUnresolvedAssembly> systemAssembly = new Lazy<IUnresolvedAssembly>(
            delegate
            {
                var loader = new CecilLoader();
                loader.IncludeInternalMembers = true;
                return loader.LoadAssemblyFile(typeof(System.ComponentModel.BrowsableAttribute).Assembly.Location);
            });
        static readonly Lazy<IUnresolvedAssembly> systemXmlLinq = new Lazy<IUnresolvedAssembly>(
            delegate
            {
                var loader = new CecilLoader();
                loader.IncludeInternalMembers = true;
                return loader.LoadAssemblyFile(typeof(System.Xml.Linq.XElement).Assembly.Location);
            });


        static readonly Lazy<IUnresolvedAssembly> mscorlib = new Lazy<IUnresolvedAssembly>(
            delegate
            {
                var loader = new CecilLoader();
                loader.IncludeInternalMembers = true;
                return loader.LoadAssemblyFile(typeof(object).Assembly.Location);
            });

        static readonly Lazy<IUnresolvedAssembly> systemCore = new Lazy<IUnresolvedAssembly>(
            delegate
            {
                var loader = new CecilLoader();
                loader.IncludeInternalMembers = true;
                return loader.LoadAssemblyFile(typeof(System.Linq.Enumerable).Assembly.Location);
            });

        // i'm not using this...
        public static CSharpCompletionEngine CreateEngine(string text, out int cursorPosition, params IUnresolvedAssembly[] references)
        {
            string parsedText;
            string editorText;
            cursorPosition = text.IndexOf('$');
            int endPos = text.IndexOf('$', cursorPosition + 1);
            if (endPos == -1)
            {
                if (cursorPosition < 0)
                {
                    parsedText = editorText = text;
                }
                else
                {
                    parsedText = editorText = text.Substring(0, cursorPosition) + text.Substring(cursorPosition + 1);
                }
            }
            else
            {
                parsedText = text.Substring(0, cursorPosition) + new string(' ', endPos - cursorPosition) + text.Substring(endPos + 1);
                editorText = text.Substring(0, cursorPosition) + text.Substring(cursorPosition + 1, endPos - cursorPosition - 1) + text.Substring(endPos + 1);
                cursorPosition = endPos - 1;
            }
            var doc = new ReadOnlyDocument(editorText);

            IProjectContent projContent = new CSharpProjectContent();
            var refs = new List<IUnresolvedAssembly> { mscorlib.Value, systemCore.Value, systemAssembly.Value, systemXmlLinq.Value };
            if (references != null)
                refs.AddRange(references);

            projContent = projContent.AddAssemblyReferences(refs);

            // Parse => SyntaxTree
            var syntaxTree = new CSharpParser().Parse(parsedText, "program.cs");
            syntaxTree.Freeze();

            var unresolvedFile = syntaxTree.ToTypeSystem();
            // Add CSharpUnresolvedFile to CSharpProjectContent
            projContent = projContent.AddOrUpdateFiles(unresolvedFile);

            // Create a TypeSystem.ICompilation that allows resolving within the project.
            var compilation = projContent.CreateCompilation();
            var textCursorLocation = cursorPosition > 0 ? doc.GetLocation(cursorPosition) : new TextLocation(1, 1);


            #region Create and Refine the type resolution context as much as possible 

            var typeResolveContext = new CSharpTypeResolveContext(compilation.MainAssembly);
            typeResolveContext = typeResolveContext.WithUsingScope(unresolvedFile.GetUsingScope(textCursorLocation).Resolve(compilation));

            var curDef = unresolvedFile.GetInnermostTypeDefinition(textCursorLocation);
            if (curDef != null)
            {
                var resolvedDef = curDef.Resolve(typeResolveContext).GetDefinition();
                typeResolveContext = typeResolveContext.WithCurrentTypeDefinition(resolvedDef);
                var curMember = resolvedDef.Members.FirstOrDefault(m => m.Region.Begin <= textCursorLocation && textCursorLocation < m.BodyRegion.End);
                if (curMember != null)
                {
                    typeResolveContext = typeResolveContext.WithCurrentMember(curMember);
                }
            }

            #endregion
            
            // Cool!  Marry the concept of content & typed 
            var completionContext = new DefaultCompletionContextProvider(doc, unresolvedFile);
            #region Add Preprocessor Symbols
            completionContext.AddSymbol("TEST");
            foreach (var sym in syntaxTree.ConditionalSymbols)
            {
                completionContext.AddSymbol(sym);
            }
            #endregion
            var engine = new CSharpCompletionEngine(doc, completionContext, new TestCompletionDataFactory(new CSharpResolver(typeResolveContext)), projContent, typeResolveContext);

            engine.EolMarker = Environment.NewLine;
            engine.FormattingPolicy = FormattingOptionsFactory.CreateMono();
            return engine;
        }

        public static CompletionDataList CreateProvider(string text, bool isCtrlSpace, params IUnresolvedAssembly[] references)
        {
            int cursorPosition;
            var engine = CreateEngine(text, out cursorPosition, references);
            var data = engine.GetCompletionData(cursorPosition, isCtrlSpace);

            return new CompletionDataList()
            {
                Data = data,
                AutoCompleteEmptyMatch = engine.AutoCompleteEmptyMatch,
                AutoSelect = engine.AutoSelect,
                DefaultCompletionString = engine.DefaultCompletionString
            };
        }
		

    }
#endif
}
