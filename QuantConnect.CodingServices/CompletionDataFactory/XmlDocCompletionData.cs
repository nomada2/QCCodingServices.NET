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
        public class XmlDocCompletionData : CompletionData
        {
            public string Title { get; private set; }
            //public string Description { get; private set; }

            public XmlDocCompletionData(string title, string description, string insertText)
            {
                SetDefaultText(insertText);
                DeclarationCategory = DeclarationCategory.XmlDocumentation;

                Title = title;
                Description = description;

                //Description = currentMember
            }
        }

        public ICompletionData CreateXmlDocCompletionData(string title, string description, string insertText)
        {
            var cd = new XmlDocCompletionData(title, description, insertText);
            // Needs confirmation
            return cd;
        }

    }
}
