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
        /// <summary>
        /// Represents a namespace
        /// </summary>
        public class NamespaceCompletionData : CompletionData
        {
            public INamespace Namespace { get; private set; }

            public NamespaceCompletionData(INamespace ns)
            {
                DeclarationCategory = DeclarationCategory.Namespace;
                Namespace = ns;
                SetDefaultText(ns.Name);
            }
        
        }

        public ICompletionData CreateNamespaceCompletionData(INamespace ns)
        {
            var cd = new NamespaceCompletionData(ns);
            return cd;
        }

    }

}
