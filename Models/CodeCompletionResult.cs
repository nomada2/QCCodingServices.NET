using System.Linq;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;
using Newtonsoft.Json;
using QuantConnect.Server.Autocomplete.CompletionDataFactory;

namespace QuantConnect.Server.Autocomplete.Models
{
    /// <summary>
    /// This guy will get his data from CodeCompletionDataFactory.CompletionData
    /// </summary>
    public class CodeCompletionResult
    {
        /// <summary>
        /// A unique identifier (in the context of a single response?)
        /// NOTE: This is required by the client code
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// The derived declaration category.
        /// </summary>
        [JsonIgnore]  // leave as ignore.  The property below will be used for serialization for now (temporarily).
        public DeclarationCategory DeclarationCategory { get; set; }
        [JsonProperty("sDeclarationCategory")]
        public string DeclarationCategoryName
        {
            get { return DeclarationCategory.ToString(); }
        }

        /// <summary>
        /// Display text to represent the code completion option in the code completion list
        /// </summary>
        [JsonProperty("sName")]
        public string DisplayText { get; set; }

        /// <summary>
        /// Completion text represented by this option.
        /// For most options, this will be the same as the display text, but for snippets and the like, this will obviously be much more than that.
        /// </summary>
        [JsonProperty("sCode")]
        public string CompletionText { get; set; }

        /// <summary>
        /// (optional) A [more comprehensive] description of the code completion option, intended
        /// to be used for tooltip content.  This could contain information about the return type of the member,
        /// the type in which the member resides, domentation about the member, or anything else...
        /// </summary>
        [JsonProperty("sDescription")]
        public string Description { get; set; }
        
        /// <summary>
        /// Indicates the number of overloads which exist for the completion option.
        /// NOTE: ONLY applies to C# completion options for which overloads can exist (i.e. methods)
        /// </summary>
        [JsonIgnore]
        public int OverloadCount { get; set; }
        
        /// <summary>
        /// The name of the type inside of which this member is declared.
        /// Applicable to members of types.
        /// </summary>
        [JsonProperty("sMemberDeclaringType")]
        public string MemberDeclaringType { get; set; }

        /// <summary>
        /// The name of the declared result type of this member.  
        /// For properties and fields, it will be the declared type; for methods, it will be the declared return type.
        /// Applicable to members of types.
        /// </summary>
        [JsonProperty("sMemberType")]
        public string MemberDeclaredResultType { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}  {2}  {3} {4} \"{5}\"",
                (string.IsNullOrWhiteSpace(MemberDeclaringType) ? "" : string.Format("[memberof:{0}] ", MemberDeclaringType)), 
                DeclarationCategory,
                MemberDeclaredResultType,
                DisplayText, 
                (OverloadCount == 0 ? "" : " ("+OverloadCount+" overloads) "), 
                Description);
        }
    }


    public static class CodeCompletionResultUtility
    {
        public static CodeCompletionResult FromICompletionDataToFileCodeCompletionResult(this ICompletionData completionData)
        {
            CodeCompletionResult result = new CodeCompletionResult();

            // Set Defaults
            result.CompletionText = completionData.CompletionText;
            result.DisplayText = completionData.DisplayText;
            result.Description = completionData.Description;
            result.OverloadCount = completionData.OverloadedData.Count();

            result.MemberDeclaringType = "";
            result.MemberDeclaredResultType = "";

            CodeCompletionDataFactory.CompletionData cd = (CodeCompletionDataFactory.CompletionData) completionData;
            result.DeclarationCategory = cd.DeclarationCategory;

            // For EntityCompletionData, if the entity is an IMember, NRefactory sets the CompletionCategory's DisplayText to the name of the class (i.e. member.DeclaringTypeDefinintion.Name)
            if (completionData.CompletionCategory != null)
                result.MemberDeclaringType = completionData.CompletionCategory.DisplayText;
            // Let's see if we can reproduce 
            var ecd = cd as CodeCompletionDataFactory.EntityCompletionData;
            if (ecd != null)
            {
                var ecdmem = ecd.Entity as IMember;
                if (ecdmem != null)
                {
                    result.MemberDeclaringType = ecdmem.DeclaringTypeDefinition.FullName;
                    result.MemberDeclaredResultType = ecdmem.MemberDefinition.ReturnType.FullName;
                }
            }


            return result;
        }

    }
}