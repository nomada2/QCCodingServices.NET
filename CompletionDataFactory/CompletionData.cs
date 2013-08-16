﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Completion;

namespace QuantConnect.Server.Autocomplete.CompletionDataFactory
{
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

            public DeclarationCategory DeclarationCategory { get; set; }
            //public IType MemberDeclaringType { get; set; }
            //public ITypeDefinition MemberDeclaringTypeDefinition { get; set; }

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
                DisplayText = CompletionText = Description = text;
            }
        }


    }
}
