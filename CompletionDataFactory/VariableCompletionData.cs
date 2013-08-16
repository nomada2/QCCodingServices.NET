using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;

namespace QuantConnect.Server.Autocomplete.CompletionDataFactory
{
    public partial class CodeCompletionDataFactory
    {
        public class VariableCompletionData : CompletionData, IVariableCompletionData
        {
            public IVariable Variable { get; private set; }

            public VariableCompletionData(IVariable variable)
            {
                Variable = variable;
                SetDefaultText(variable.Name);
                // Needs confirmation
                DeclarationCategory = DeclarationCategory.Variable;
            }
        }

        public ICompletionData CreateVariableCompletionData(IVariable variable)
        {
            var cd = new VariableCompletionData(variable);
            return cd;
        }

        // -----------------------------------------------------------------

        public ICompletionData CreateVariableCompletionData(ITypeParameter parameter)
        {
            var cd = new CompletionData(parameter.Name);
            // Needs confirmation
            cd.DeclarationCategory = DeclarationCategory.TypeParameter;
            return cd;
        }

    }

}
