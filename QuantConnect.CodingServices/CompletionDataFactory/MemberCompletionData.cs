using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;

namespace QuantConnect.CodingServices.CompletionDataFactory
{
    public partial class CodeCompletionDataFactory
    {
        /// <summary>
        /// Represents a member of an enumeration
        /// Technically, this could have been derived from EntityCompletionData; that's what the NRefactory tests do,
        /// but I felt it should be its own completion data type.
        /// </summary>
        public class MemberCompletionData : CompletionData
        {
            public IType Type { get; private set; }
            public IEntity Member { get; private set; }

            public MemberCompletionData(IType type, IEntity member, TypeSystemAstBuilder builder)
            {
                DeclarationCategory = DeclarationCategory.Enumeration_Member;

                Type = type;
                Member = member;

                string typeName = builder.ConvertType(type).GetText();
                SetDefaultText(typeName + "." + member.Name);

                Documentation = member.Documentation;
            }
            
        }

        /// <summary>
        /// This is invoked from AddEnumMembers (which is invoked in several other contexts).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public ICompletionData CreateMemberCompletionData(IType type, IEntity member)
        {
            var cd = new MemberCompletionData(type, member, builder);
            return cd;
        }


    }
}
