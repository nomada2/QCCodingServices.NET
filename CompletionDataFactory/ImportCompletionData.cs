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
        public class ImportCompletionData : CodeCompletionDataFactory.CompletionData
        {
            public IType Type { get; private set; }

            public bool UseFullName { get; private set; }

            public ImportCompletionData(IType type, bool useFullName) //: base(type.Name)
            {
                Type = type;
                UseFullName = useFullName;

                //DeclarationCategory = ???
                
                SetDefaultText(type.Name);
            }
        }

        /// <summary>
        /// Called directly by the CSharpCompletionEngine.GetImportCompletionData public method.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="useFullName"></param>
        /// <returns></returns>
        public ICompletionData CreateImportCompletionData(IType type, bool useFullName)
        {
            var cd = new ImportCompletionData(type, useFullName);
            return cd;
        }

    }
}
