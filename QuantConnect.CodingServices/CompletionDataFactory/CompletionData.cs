using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Completion;

namespace QuantConnect.CodingServices.CompletionDataFactory
{
    /// <summary>
    /// CodeCompletionDataFactory
    /// </summary>
    public partial class CodeCompletionDataFactory
    {
        public class CompletionData : ICompletionData
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

            public IEnumerable<ICompletionData> OverloadedData
            {
                get { return overloadedData; }
                set { throw new NotImplementedException(); }
            }

            #endregion

            #region Custom Additions

            /// <summary>
            /// This tells us something about how the option was declared (if it was declared at all).
            /// It is intended to provide a useful cue for icons to be associated with the autocomplete options.
            /// </summary>
            public DeclarationCategory DeclarationCategory { get; set; }

            /// <summary>
            /// This is to allow explicit differentiation of documentation from some other description.
            /// </summary>
            public string Documentation { get; set; }

            #endregion

            /// <summary>
            /// Subclasses may opt to use this constructor variant
            /// </summary>
            protected CompletionData()
            {}

            public CompletionData(/*DeclarationCategory declarationCategory,*/ string text)
            {
                SetDefaultText(text);
            }

            /// <summary>
            /// Convenience method to set the DisplayText, CompletionText, and Description to the same value.
            /// </summary>
            /// <param name="text"></param>
            protected void SetDefaultText(string text)
            {
                DisplayText = CompletionText = text;
                // Don't set description to this text.  That will be addressed at a later time...
            }
        }


    }
}
