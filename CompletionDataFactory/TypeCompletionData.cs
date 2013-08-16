using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.Completion;

namespace QuantConnect.Server.Autocomplete.CompletionDataFactory
{
    public partial class CodeCompletionDataFactory
    {
        public class TypeCompletionData : CompletionData
        {
            public ICSharpCode.NRefactory.TypeSystem.IType Type { get; private set; }
            public bool FullName { get; private set; }
            public bool IsInAttributeContext { get; private set; }

            public TypeCompletionData(ICSharpCode.NRefactory.TypeSystem.IType type, bool fullName, bool isInAttributeContext, TypeSystemAstBuilder builder)
            {
                Type = type;
                FullName = fullName;
                IsInAttributeContext = isInAttributeContext;

                // Confirm that this doesn't also include other types, such as enums, structs, etc.
                //DeclarationCategory = DeclarationCategory.Class;
                DeclarationCategory = type.Kind.ResolveDeclarationCategoryFromTypeKind();

                string typeName = fullName ? builder.ConvertType(type).GetText() : type.Name;
                if (isInAttributeContext && typeName.EndsWith("Attribute") && typeName.Length > "Attribute".Length)
                {
                    typeName = typeName.Substring(0, typeName.Length - "Attribute".Length);
                }
                SetDefaultText(typeName);

                // for documentation, see type.GetDefinition().Documentation
                Description = type.GetDefinition().Documentation;
            }

        }

        public ICompletionData CreateTypeCompletionData(ICSharpCode.NRefactory.TypeSystem.IType type, bool fullName, bool isInAttributeContext)
        {
            var cd = new TypeCompletionData(type, fullName, isInAttributeContext, builder);
            return cd;
        }

    }
}
