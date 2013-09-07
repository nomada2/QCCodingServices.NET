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
        public class EventCompletionData : CompletionData
        {
            public IType DelegateType { get; private set; }
            public IEvent Event { get; private set; }
            public IUnresolvedMember CurrentMember { get; private set; }
            public IUnresolvedTypeDefinition CurrentType { get; private set; }

            public EventCompletionData(string varName, IType delegateType, IEvent evt, string parameterDefinition, IUnresolvedMember currentMember, IUnresolvedTypeDefinition currentType)
            {
                SetDefaultText(varName);
                DeclarationCategory = DeclarationCategory.Event;
                // what should we do with parameterDefinition???
                DelegateType = delegateType;
                CurrentMember = currentMember;
                CurrentType = currentType;

                //Description = currentMember
            }
        }

        public ICompletionData CreateEventCreationCompletionData(string varName, IType delegateType, IEvent evt, string parameterDefinition, IUnresolvedMember currentMember, IUnresolvedTypeDefinition currentType)
        {
            var cd = new EventCompletionData(varName, delegateType, evt, parameterDefinition, currentMember, currentType);
            // Needs confirmation
            return cd;
        }

    }
}
