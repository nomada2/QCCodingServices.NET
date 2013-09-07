// 
// TestCompletionDataFactory.cs
//  
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc. (http://xamarin.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;

namespace qc.server.autocomplete
{
    //public class QCCompletionCategory : CompletionCategory
    //{
    //    public QCCompletionCategory(string text, string icon) : base(text, icon)
    //    {
            
    //    }

    //    public override int CompareTo(CompletionCategory other)
    //    {
    //        int result1 = Icon.CompareTo(other.Icon);
    //        if (result1 != 0)
    //            return result1;

    //        return DisplayText.CompareTo(other.DisplayText);
    //    }
    //}

    /// <summary>
    /// Indicates how the completion option is declared
    /// </summary>
    public enum DeclarationCategory
    {
        NotSet = 0,

        Literal,
        Namespace,
        Enum,
        Struct,
        Interface,
        Class,
        StaticClass,
        AbstractClass,
        Field,
        Property,
        Method,
        MethodParameter,
        Delegate,
        TypeParameter,
    }

    public class TestCompletionDataFactory : ICompletionDataFactory
    {
        readonly CSharpResolver state;
        readonly TypeSystemAstBuilder builder;

        public TestCompletionDataFactory(CSharpResolver state)
        {
            this.state = state;
            builder = new TypeSystemAstBuilder(state);
            builder.ConvertUnboundTypeArguments = true;
        }

        public class CompletionData
            : ICompletionData
        {
            #region ICompletionData implementation
            public void AddOverload(ICompletionData data)
            {
                if (overloadedData.Count == 0)
                    overloadedData.Add(this);
                overloadedData.Add(data);
            }

            /// <summary>
            /// It turns out that NRefactory sets the CompletionCategory for members of classes.
            /// As a result, we can't really do anything useful with this property across all 
            /// completion data types, because we can't rely on it retaining the data we assign
            /// to it.  Bummer!
            /// </summary>
            public CompletionCategory CompletionCategory { get; set; }

            public string DisplayText { get; set; }

            public string Description { get; set; }

            public string CompletionText { get; set; }

            public DisplayFlags DisplayFlags { get; set; }

            public bool HasOverloads
            {
                get { return overloadedData.Count > 0; }
            }

            List<ICompletionData> overloadedData = new List<ICompletionData>();

            public System.Collections.Generic.IEnumerable<ICompletionData> OverloadedData
            {
                get { return overloadedData; }
                set { throw new NotImplementedException(); }
            }

            #endregion

            public DeclarationCategory DeclarationCategory { get; set; }

            public CompletionData(/*DeclarationCategory declarationCategory,*/ string text)
            {
                //DeclarationCategory = declarationCategory;
                DisplayText = CompletionText = Description = text;
                //CompletionCategory = new QCCompletionCategory("---", "@@@");
            }
        }

        public class EntityCompletionData : CompletionData, IEntityCompletionData
        {
            #region IEntityCompletionData implementation

            public IEntity Entity
            {
                get;
                private set;
            }

            #endregion

            public EntityCompletionData(IEntity entity)
                : this(entity, entity.Name)
            {
            }

            public EntityCompletionData(IEntity entity, string txt)
                : base(txt)
            {
                this.Entity = entity;
            }
        }

        public class ImportCompletionData : CompletionData
        {
            public IType Type
            {
                get;
                private set;
            }

            public bool UseFullName
            {
                get;
                private set;
            }

            public ImportCompletionData(IType type, bool useFullName)
                : base(type.Name)
            {
                this.Type = type;
                this.UseFullName = useFullName;
            }
        }

        #region ICompletionDataFactory implementation
        public ICompletionData CreateEntityCompletionData(ICSharpCode.NRefactory.TypeSystem.IEntity entity)
        {
            return new EntityCompletionData(entity);
        }

        public ICompletionData CreateEntityCompletionData(ICSharpCode.NRefactory.TypeSystem.IEntity entity, string text)
        {
            return new CompletionData(text);
        }

        public ICompletionData CreateEntityCompletionData(ICSharpCode.NRefactory.TypeSystem.IUnresolvedEntity entity)
        {
            return new CompletionData(entity.Name);
        }

        public ICompletionData CreateTypeCompletionData(ICSharpCode.NRefactory.TypeSystem.IType type, bool fullName, bool isInAttributeContext)
        {
            string name = fullName ? builder.ConvertType(type).GetText() : type.Name;
            if (isInAttributeContext && name.EndsWith("Attribute") && name.Length > "Attribute".Length)
            {
                name = name.Substring(0, name.Length - "Attribute".Length);
            }
            return new CompletionData(name);
        }

        public ICompletionData CreateMemberCompletionData(IType type, IEntity member)
        {
            string name = builder.ConvertType(type).GetText();
            return new EntityCompletionData(member, name + "." + member.Name);
        }


        public ICompletionData CreateLiteralCompletionData(string title, string description, string insertText)
        {
            return new CompletionData(title);
        }

        public ICompletionData CreateNamespaceCompletionData(INamespace ns)
        {
            return new CompletionData(ns.Name);
        }

        public ICompletionData CreateVariableCompletionData(ICSharpCode.NRefactory.TypeSystem.IVariable variable)
        {
            return new CompletionData(variable.Name);
        }

        public ICompletionData CreateVariableCompletionData(ICSharpCode.NRefactory.TypeSystem.ITypeParameter parameter)
        {
            return new CompletionData(parameter.Name);
        }

        public ICompletionData CreateEventCreationCompletionData(string varName, ICSharpCode.NRefactory.TypeSystem.IType delegateType, ICSharpCode.NRefactory.TypeSystem.IEvent evt, string parameterDefinition, ICSharpCode.NRefactory.TypeSystem.IUnresolvedMember currentMember, ICSharpCode.NRefactory.TypeSystem.IUnresolvedTypeDefinition currentType)
        {
            return new CompletionData(varName);
        }

        public ICompletionData CreateNewOverrideCompletionData(int declarationBegin, ICSharpCode.NRefactory.TypeSystem.IUnresolvedTypeDefinition type, ICSharpCode.NRefactory.TypeSystem.IMember m)
        {
            return new CompletionData(m.Name);
        }

        public ICompletionData CreateNewPartialCompletionData(int declarationBegin, IUnresolvedTypeDefinition type, IUnresolvedMember m)
        {
            return new CompletionData(m.Name);
        }

        public ICompletionData CreateImportCompletionData(IType type, bool useFullName)
        {
            return new ImportCompletionData(type, useFullName);
        }

        public System.Collections.Generic.IEnumerable<ICompletionData> CreateCodeTemplateCompletionData()
        {
            return Enumerable.Empty<ICompletionData>();
        }

        public IEnumerable<ICompletionData> CreatePreProcessorDefinesCompletionData()
        {
            yield return new CompletionData("DEBUG");
            yield return new CompletionData("TEST");
        }
        #endregion
    }
}