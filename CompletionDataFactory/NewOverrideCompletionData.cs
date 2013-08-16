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
        public class NewOverrideCompletionData : CompletionData
        {
            public int DeclarationBeginningIndex { get; private set; }
            public IUnresolvedTypeDefinition Type { get; private set; }
            public IMember Member { get; private set; }

            public NewOverrideCompletionData(int declarationBegin, IUnresolvedTypeDefinition type, IMember member)
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
        /// Called indirectly by the CSharpCompletionEngine.GetOverrideCompletionData method.
        /// </summary>
        /// <param name="declarationBegin"></param>
        /// <param name="type"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public ICompletionData CreateNewOverrideCompletionData(int declarationBegin, IUnresolvedTypeDefinition type, IMember member)
        {
            var cd = new NewOverrideCompletionData(declarationBegin, type, member);
            return cd;
        }

    }
}
