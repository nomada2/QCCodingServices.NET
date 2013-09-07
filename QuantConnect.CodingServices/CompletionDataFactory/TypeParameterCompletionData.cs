using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;

namespace QuantConnect.CodingServices.CompletionDataFactory
{
    public partial class CodeCompletionDataFactory
    {
        public class TypeParameterCompletionData : CompletionData
        {
            public ITypeParameter TypeParameter { get; private set; }

            public TypeParameterCompletionData(ITypeParameter typeParameter)
            {
                TypeParameter = typeParameter;
                SetDefaultText(typeParameter.Name);
                DeclarationCategory = DeclarationCategory.Type_Parameter;
                //Documentation = typeParameter.GetDefinition().Documentation;
            }
        }

        public ICompletionData CreateVariableCompletionData(ITypeParameter typeParameter)
        {
            var cd = new TypeParameterCompletionData(typeParameter);
            return cd;
        }
    }
}
