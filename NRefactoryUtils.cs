using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Documentation;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using QuantConnect.Server.Autocomplete.CompletionDataFactory;
using QuantConnect.Server.Autocomplete.Models;

using QuantConnect;
using QuantConnect.Securities;

namespace QuantConnect.Server.Autocomplete
{
    public static class NRefactoryUtils
    {
        public static void LoadReferencesInBackground()
        {
            Task.Factory.StartNew(() =>
            {
                var foo = QCReferences.Value;
            });
        }

        public static FileParseResult[] ParseFile(ProjectFileModel fileModel)
        {
            var parser = new CSharpParser();
            SyntaxTree syntaxTree = parser.Parse(fileModel.Content, fileModel.Name);

            var results = parser.ErrorsAndWarnings
                .Select(x => new FileParseResult()
                {
                    FileId = fileModel.Id,
                    FileName = fileModel.Name,
                    Line = x.Region.BeginLine,
                    Column = x.Region.BeginColumn,
                    Type = x.ErrorType,
                    Message = x.Message
                }).ToArray();

            return results;
        }

        public static ProjectAnalysisResult RunFullProjectAnalysis(ProjectAnalysisRequest projectAnalysisRequest) // ProjectModel projectModel, int fileId, int line, int column)
        {
            Stopwatch sw = Stopwatch.StartNew();

            ProjectAnalysisResult projectAnalysisResult = new ProjectAnalysisResult();
            ProjectModel projectModel = projectAnalysisRequest.ProjectModel;


            // Set up the project (if not already done)
            if (projectModel.ProjectContent == null)
            {
                projectModel.ProjectContent = new CSharpProjectContent();
                projectModel.ProjectContent = projectModel.ProjectContent.AddAssemblyReferences(QCReferences.Value);
            }

            // For each new file, we need to integrate it into the project content
            var fileModelsInProject = projectModel.GetFileDescendants().ToArray();
            foreach (var fileModelInProject in fileModelsInProject)
            {
                IntegrateFileModel(projectModel, fileModelInProject);
            }

            // We can return now if no code completion was requested
            if (projectAnalysisRequest.CodeCompletionParameters == null)
            {
                projectAnalysisResult.TimeElapsed = sw.Elapsed;
                return projectAnalysisResult;
            }

            // Now it's time to give attention specifically to the matter of resolving the code completion
            // options.  This, of course, requires a deeper analysis of the specified file...
            
            var codeCompletionParams = projectAnalysisRequest.CodeCompletionParameters;

            // Locate the file in the project
            ProjectFileModel fileModel = projectModel.FindFile(codeCompletionParams.FileId);
            if (fileModel == null)
                throw new Exception("Specified file does not exist in this project");
            

            // Create a TypeSystem.ICompilation that allows resolving within the project.
            var compilation = projectModel.ProjectContent.CreateCompilation();

            #region Resolve text cursor/caret location
            
            // The text cursor position is crucial to creating a properly-scoped type resolution context
            // so as to get relevant code completion suggestions.
            int textCursorOffset = 0;
            TextLocation textCursorLocation = new TextLocation(1, 1);
            ReadOnlyDocument doc = new ReadOnlyDocument(fileModel.Content);
            try
            {
                // if line and column aren't set, we'll assume that the cursor offset/index is set
                if (codeCompletionParams.Line == 0 && codeCompletionParams.Column == 0)
                {
                    textCursorOffset = codeCompletionParams.Offset;
                    if (textCursorOffset < 0) textCursorOffset = 0;
                    textCursorLocation = doc.GetLocation(textCursorOffset);
                }
                // if either line or column are invalid (i.e. <= 0), then we'll use offset 0 instead
                else if (codeCompletionParams.Line <= 0 || codeCompletionParams.Column <= 0)
                {
                    textCursorOffset = 0;
                }
                else
                {
                    textCursorLocation = new TextLocation(codeCompletionParams.Line, codeCompletionParams.Column);
                    textCursorOffset = doc.GetOffset(textCursorLocation);
                    codeCompletionParams.Offset = textCursorOffset;
                }
            }
            catch (Exception)
            {
                textCursorOffset = 0;
                textCursorLocation = new TextLocation(1, 1);
            }
            finally
            {
                projectAnalysisResult.Line = textCursorLocation.Line;
                projectAnalysisResult.Column = textCursorLocation.Column;
                projectAnalysisResult.Offset = textCursorOffset;
            }

            #endregion

            #region Create and Refine the type resolution context as much as possible based upon the cursor position

            var typeResolveContext = new CSharpTypeResolveContext(compilation.MainAssembly);
            // Constrain the resolve context by using scope
            typeResolveContext = typeResolveContext
                .WithUsingScope(fileModel.UnresolvedFile.GetUsingScope(textCursorLocation)
                .Resolve(compilation));

            var curDef = fileModel.UnresolvedFile.GetInnermostTypeDefinition(textCursorLocation);
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

            // The purpose of the rest of these steps is a little fuzzy in my mind...  
            // I'm still trying to understand them fully and if/why they're all needed.
            // It seems there is some redundancy here...

            var completionContext = new DefaultCompletionContextProvider(doc, fileModel.UnresolvedFile);

            #region Add Preprocessor Symbols??
            completionContext.AddSymbol("TEST");
            foreach (var sym in fileModel.SyntaxTree.ConditionalSymbols)
                completionContext.AddSymbol(sym);
            #endregion

            var completionDataFactory = new CodeCompletionDataFactory(new CSharpResolver(typeResolveContext));
            
            var completionEngine = new CSharpCompletionEngine(doc, completionContext, completionDataFactory, projectModel.ProjectContent, typeResolveContext);
            completionEngine.EolMarker = Environment.NewLine;
            completionEngine.FormattingPolicy = FormattingOptionsFactory.CreateMono();
            projectModel.CompletionEngine = completionEngine;

GetDocumentContext(textCursorOffset, doc);

            // Finally, generate completion data!
            var completionOptions = completionEngine.GetCompletionData(textCursorOffset, projectAnalysisRequest.CodeCompletionParameters.CtrlSpace).ToArray();
            projectAnalysisResult.CompletionOptions = completionOptions.OrderBy(x => x.CompletionText).ToArray();
            projectAnalysisResult.AutoCompleteEmptyMatch = completionEngine.AutoCompleteEmptyMatch;
            projectAnalysisResult.AutoSelect = completionEngine.AutoSelect;
            projectAnalysisResult.DefaultCompletionString = completionEngine.DefaultCompletionString;

            int startPos, wordLength;
            if (completionEngine.TryGetCompletionWord(textCursorOffset, out startPos, out wordLength))
            {
                //Debug.WriteLine("TryGetCompletionWord :: startpos:{0}  wordlength:{1}", startPos, wordLength);
                string completionWord = projectAnalysisResult.CompletionWord = doc.GetText(startPos, wordLength);

                if (!string.IsNullOrWhiteSpace(completionWord))
                {
                    var bestMatch = projectAnalysisResult.CompletionOptions
                        .FirstOrDefault(x => x.CompletionText.CompareTo(completionWord) >= 0);
                    projectAnalysisResult.BestMatchToCompletionWord = bestMatch;
                    //if (bestMatch != null)
                        //projectAnalysisResult.BestMatchToCompletionWord = bestMatch.CompletionText;
                }

            }

            projectAnalysisResult.TimeElapsed = sw.Elapsed;

            return projectAnalysisResult;
        }

        private static void GetDocumentContext(int textCursorOffset, ReadOnlyDocument doc)
        {
            #region Debugging Aid

            // Resolve content around text cursor
            int numberOfCharactersAroundCursorToResolve = 20;
            int firstCharOffset = textCursorOffset - numberOfCharactersAroundCursorToResolve/2;
            if (firstCharOffset < 0)
                firstCharOffset = 0;
            if (doc.TextLength < firstCharOffset + numberOfCharactersAroundCursorToResolve)
                numberOfCharactersAroundCursorToResolve = doc.TextLength - firstCharOffset;
            string surroundingText = doc.GetText(firstCharOffset, numberOfCharactersAroundCursorToResolve);
            Debug.WriteLine("Text around cursor: [{0}]", surroundingText);

            #endregion
        }

        public static void IntegrateFileModel(ProjectModel projectModel, ProjectFileModel fileModel)
        {
            // Parse C# code file => SyntaxTree
            // NOTE:  The NRefactory Test project uses a somewhat "cleaner"/more "sanitized" version of the 
            // file content for creating the AST.  Why?  I can't be certain, but I suspect that this is to 
            // simplify testing by eliminating stuff that contribute to errors... but maybe there's something 
            // more to it.  Doing some more routine tests on our end should prove whether omitting this step 
            // actually makes any difference.
            fileModel.Parser = new CSharpParser();
            fileModel.SyntaxTree = fileModel.Parser.Parse(fileModel.Content, fileModel.Name);
            fileModel.SyntaxTree.Freeze();
            
            // Convert syntax tree into parsed file that can be stored in the type system.
            fileModel.UnresolvedFile = fileModel.SyntaxTree.ToTypeSystem();

            // Add specified file to the project content.  
            // If a file with the same name already exists, this will update the existing file.
            projectModel.ProjectContent = projectModel.ProjectContent.AddOrUpdateFiles(fileModel.UnresolvedFile);
        }

        static Lazy<IList<IUnresolvedAssembly>> QCReferences = new Lazy<IList<IUnresolvedAssembly>>(
            delegate
            {
                Assembly[] assemblies = {
					        typeof(object).Assembly, // mscorlib
					        typeof(Uri).Assembly, // System.dll
					        typeof(System.Linq.Enumerable).Assembly, // System.Core.dll
                            typeof(IAlgorithm).Assembly,  // QuantConnect.Algorithm.Interface.dll
                            typeof(ISecurityTransactionModel).Assembly,  // QuantConnect.Common.dll
					        //typeof(ICSharpCode.NRefactory.TypeSystem.IProjectContent).Assembly,
                            //typeof(System.Xml.XmlDocument).Assembly, // System.Xml.dll
                            //typeof(System.Drawing.Bitmap).Assembly, // System.Drawing.dll
                            //typeof(Form).Assembly, // System.Windows.Forms.dll
				        };

                IUnresolvedAssembly[] projectContents = new IUnresolvedAssembly[assemblies.Length];
                Stopwatch total = Stopwatch.StartNew();
                Parallel.For(0, assemblies.Length, delegate(int i)
                    {
                        Stopwatch w = Stopwatch.StartNew();
                        CecilLoader loader = new CecilLoader();
                        Assembly asm = assemblies[i];
                        string assemblyLocation = asm.Location;
                        string asmXmlFilePath = Path.ChangeExtension(assemblyLocation, ".xml");
                        if (File.Exists(asmXmlFilePath))
                            loader.DocumentationProvider = new XmlDocumentationProvider(asmXmlFilePath);
                        else
                            Debug.WriteLine("XML documentation file \"{0}\" does not exist.", asmXmlFilePath);
                        //loader.IncludeInternalMembers = true;
                        projectContents[i] = loader.LoadAssemblyFile(assemblyLocation);
                        Debug.WriteLine(Path.GetFileName(assemblyLocation) + ": " + w.Elapsed);
                    });
                Debug.WriteLine("Total: " + total.Elapsed);
                return projectContents;
            });

    }
}
