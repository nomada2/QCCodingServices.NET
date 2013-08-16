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
        public class NewPartialCompletionData : CompletionData
        {
            public int DeclarationBeginningIndex { get; private set; }
            public IUnresolvedTypeDefinition Type { get; private set; }
            public IUnresolvedMember Member { get; private set; }

            public NewPartialCompletionData(int declarationBegin, IUnresolvedTypeDefinition type, IUnresolvedMember member)
            {
                DeclarationBeginningIndex = declarationBegin;
                Type = type;
                Member = member;
                // check this:
                DeclarationCategory = type.EntityType.ResolveDeclarationCategoryFromEntityType();
                SetDefaultText(member.Name);
            }
        }
        /// <summary>
        /// Called indirectly by the CSharpCompletionEngine.GetPartialCompletionData method.
        /// </summary>
        /// <param name="declarationBegin"></param>
        /// <param name="type"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public ICompletionData CreateNewPartialCompletionData(int declarationBegin, IUnresolvedTypeDefinition type, IUnresolvedMember member)
        {
            var cd = new NewPartialCompletionData(declarationBegin, type, member);
            return cd;
        }

    }
}
