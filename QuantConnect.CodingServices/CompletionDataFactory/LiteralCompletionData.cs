using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Completion;

namespace QuantConnect.CodingServices.CompletionDataFactory
{
    public partial class CodeCompletionDataFactory
    {


        public class LiteralCompletionData : CompletionData
        {
            public LiteralCompletionData(string title, string description = null, string insertText = null)
            {
                DeclarationCategory = DeclarationCategory.Literal;

                //SetDefaultText(title);
                DisplayText = title;
                CompletionText = insertText ?? title;
                Description = description ?? title;
            }
        }

        public ICompletionData CreateLiteralCompletionData(string title, string description = null, string insertText = null)
        {
            var cd = new LiteralCompletionData(title, description, insertText);
            return cd;
        }
    }

}
