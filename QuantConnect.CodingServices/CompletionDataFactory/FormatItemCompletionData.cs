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
        public class FormatItemCompletionData : CompletionData
        {
            public string Format { get; private set; }

            public FormatItemCompletionData(string format, string description, object example)
            {
                Description = description;
                Format = format;

                string defaultText = string.Format("{0} - {1}: {2}", format, description, example);

                //DeclarationCategory = ???

                SetDefaultText(defaultText);
            }
        }

        /// <summary>
        /// Called directly by the CSharpCompletionEngine.GetImportCompletionData public method.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="useFullName"></param>
        /// <returns></returns>
        public ICompletionData CreateFormatItemCompletionData(string format, string description, object example)
        {
            var cd = new FormatItemCompletionData(format, description, example);
            return cd;
        }

    }
}
