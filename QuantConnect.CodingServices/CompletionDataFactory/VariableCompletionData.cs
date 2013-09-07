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
        /// <summary>
        /// This represents any variable in a method scope -- whether locally-defined in the method body 
        ///   or represented via a method parameter.
        /// TODO: Figure out how to differentiate method params from locally-defined variables, so they can be represented differently.
        /// </summary>
        public class VariableCompletionData : CompletionData, IVariableCompletionData
        {
            public IVariable Variable { get; private set; }

            public VariableCompletionData(IVariable variable)
            {
                Variable = variable;
                SetDefaultText(variable.Name);
                DeclarationCategory = DeclarationCategory.Local_Variable;
            }
        }

        public ICompletionData CreateVariableCompletionData(IVariable variable)
        {
            var cd = new VariableCompletionData(variable);
            return cd;
        }
    }

}
