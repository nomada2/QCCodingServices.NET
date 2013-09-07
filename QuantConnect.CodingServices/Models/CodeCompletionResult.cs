using System.Linq;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;
using Newtonsoft.Json;
using QuantConnect.CodingServices.CompletionDataFactory;

namespace QuantConnect.CodingServices.Models
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
        /// This tells us something about how the option was declared (if it was declared at all).
        /// It is intended to provide a useful cue for icons to be associated with the autocomplete options.
        /// </summary>
        [JsonIgnore]  // leave as ignore.  The property below will be used for serialization for now (temporarily).
        public DeclarationCategory DeclarationCategory { get; set; }
        [JsonProperty("sDeclarationCategory")]
        public string DeclarationCategoryName
        {
            get { return DeclarationCategory.ToString().Replace('_', ' '); }
        }

        /// <summary>
        /// Display text to represent the code completion option in the IDE's code completion list.
        /// </summary>
        [JsonProperty("sName")]
        public string DisplayText { get; set; }

        /// <summary>
        /// Completion text represented by this option.
        /// For most options, this will be the same as the display text, but for snippets and the like, 
        ///    this will almost certainly consist of much more than is displayed in the autocomplete option list.
        /// </summary>
        [JsonProperty("sCode")]
        public string CompletionText { get; set; }

        /// <summary>
        /// The summary content extracted from XML documentation comments 
        /// </summary>
        //[JsonProperty("sSummary")]
        [JsonIgnore]
        public string Summary { get; set; }

        /// <summary>
        /// (optional) A [more comprehensive] description of the code completion option, intended
        /// to be used for tooltip content.  This could contain information about the return type of the member,
        /// the type in which the member resides, domentation about the member, or anything else...
        /// </summary>
        [JsonProperty("sDescription")]
        public string Description { get; set; }


        /// <summary>
        /// Attributes which are applicable only to members of types.
        /// </summary>
        [JsonProperty("memberInfo")]
        public CodeCompletionMemberOfTypeResult MemberInformation { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]  {1}", DisplayText, Description);

            //if (MemberInformation == null)
            //{
            //    return string.Format("[{0}]  {1}\r\n{2} ",
            //        DisplayText,
            //        DeclarationCategoryName,
            //        Summary);
            //}
            //else
            //{
            //    return string.Format("[{0}]  {1} {2} {3}\r\n{4} {5}",
            //        DisplayText,
            //        DeclarationCategoryName,
            //        MemberInformation.DeclaredResultType,
            //        MemberInformation.FullName,
            //        Summary,
            //        (MemberInformation.OverloadCount == 0 ? "" : " (" + MemberInformation.OverloadCount + " overloads) "));
            //}
        }
    }


    public class CodeCompletionMemberOfTypeResult
    {
        /// <summary>
        /// The name of the type inside of which this member is declared.
        /// Applicable to members of types.
        /// </summary>
        [JsonProperty("sMemberDeclaringType")]
        public string DeclaringType { get; set; }

        /// <summary>
        /// The fuly-qualified name of the declared result type of this member.  
        /// For properties and fields, it will be the declared type; for methods, it will be the declared return type.
        /// Applicable to members of types.
        /// </summary>
        [JsonProperty("sMemberType")]
        public string DeclaredResultType { get; set; }

        /// <summary>
        /// The fully-qualified name of the member.
        /// </summary>
        [JsonIgnore]  //[JsonProperty("sFullName")]
        public string FullName { get; set; }

        /// <summary>
        /// Indicates the number of overloads which exist for the completion option.
        /// NOTE: This ONLY applies to C# completion options for which overloads can exist (i.e. methods),
        ///  but in VB.NET, indexers can also be overloaded.
        /// </summary>
        //[JsonIgnore]
        [JsonProperty("iOverloadCount")]
        public int OverloadCount { get; set; }
        
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

            //result.MemberDeclaringType = "";
            //result.MemberDeclaredResultType = "";

            CodeCompletionDataFactory.CompletionData cd = (CodeCompletionDataFactory.CompletionData) completionData;
            // Extract and capture the summary from the XML documentation for the option
            // We shouldn't be surprised that in many cases this will be empty.
            var xmlDoc = new XmlDocumentationModel(cd.Documentation);
            result.Summary = xmlDoc.Summary;


            result.DeclarationCategory = cd.DeclarationCategory;

            bool boringDescription = string.IsNullOrWhiteSpace(result.Description) || result.Description == result.CompletionText;

            
            // For EntityCompletionData, if the entity is an IMember, NRefactory sets the CompletionCategory's DisplayText to the name of the class (i.e. member.DeclaringTypeDefinintion.Name)
            //if (completionData.CompletionCategory != null)
            //    result.MemberDeclaringType = completionData.CompletionCategory.DisplayText;
            // Let's see if we can reproduce...

            var entity = cd as CodeCompletionDataFactory.EntityCompletionData;
            if (entity != null)
            {
                var member = entity.Entity as IMember;
                if (member != null)
                {
                    var memberInfo = new CodeCompletionMemberOfTypeResult();
                    result.MemberInformation = memberInfo;
                    memberInfo.FullName = member.FullName;
                    memberInfo.DeclaringType = member.DeclaringTypeDefinition.FullName;
                    memberInfo.DeclaredResultType = member.MemberDefinition.ReturnType.FullName;
                    memberInfo.OverloadCount = completionData.OverloadedData.Count();
                    
                    if (boringDescription)
                    {
                        result.Description = string.Format("{0} {1} {2}\r\n{3} {4}",
                            result.DeclarationCategoryName,
                            memberInfo.DeclaredResultType,
                            memberInfo.FullName,
                            result.Summary,
                            (memberInfo.OverloadCount == 0 ? "" : " (" + memberInfo.OverloadCount + " overloads) "));
                    }
                }
                else
                {
                    result.Description = string.Format("{0} \r\n{1}",
                        result.DeclarationCategoryName,
                        result.Summary);
                }
            }

            var ns = cd as CodeCompletionDataFactory.NamespaceCompletionData;
            if (ns != null)
            {
                result.Description = string.Format("{0} {1}", result.DeclarationCategoryName, ns.Namespace.FullName);
            }

            var variable = cd as CodeCompletionDataFactory.VariableCompletionData;
            if (variable != null)
            {
                result.Description = string.Format("{0} {1} {2}", result.DeclarationCategoryName, variable.Variable.Type.FullName, variable.Variable.Name);
            }

            var literal = cd as CodeCompletionDataFactory.LiteralCompletionData;
            if (literal != null)
            {
                if (literal.Description == literal.DisplayText)
                    result.Description = string.Format("{0} {1}", result.DeclarationCategoryName, literal.DisplayText);
                else
                    result.Description = string.Format("{0} {1}\r\n{2}", result.DeclarationCategoryName, literal.DisplayText, literal.Description);
            }

            var enumMember = cd as CodeCompletionDataFactory.MemberCompletionData;
            if (enumMember != null)
            {
                result.Description = string.Format("{0} {1}", result.DeclarationCategoryName, enumMember.DisplayText);
            }

            var typeCompletion = cd as CodeCompletionDataFactory.TypeCompletionData;
            if (typeCompletion!= null)
            {
                result.Description = string.Format("{0} {1}\r\n{2}", result.DeclarationCategoryName, typeCompletion.Type.FullName, result.Summary);
            }

            var typeParameter = cd as CodeCompletionDataFactory.TypeParameterCompletionData;
            if (typeParameter != null)
            {
                var owner = typeParameter.TypeParameter.Owner;
                if (owner != null)
                {
                    var ownerXmlDoc = new XmlDocumentationModel(owner.Documentation);
                    result.Summary = ownerXmlDoc.GetTypeParameterDescription(typeParameter.TypeParameter.Name);
                }
                result.Description = string.Format("{0} {1}\r\n{2}", result.DeclarationCategoryName, typeParameter.TypeParameter.FullName, result.Summary);
            }

            result.Description = result.Description.TrimEnd('\n', '\r');

            return result;
        }

    }

    /// <summary>
    /// For now, this class only extracts the content of the <code>summary</code> XML tag from XML documentation, 
    /// but in the future, we may want to flesh it out to extract some other common XML tags, such as <code>params</code> 
    /// and <code>returns</code>.
    /// </summary>
    class XmlDocumentationModel
    {
        private string Xml;

        public XmlDocumentationModel(string xml)
        {
            Xml = xml;
        }

        private readonly string SUMMARY_OPENING_TAG = "<summary>";
        private readonly string SUMMARY_CLOSING_TAG = "</summary>";

        public string Summary
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Xml))
                    return "";

                int startingIndex = Xml.IndexOf(SUMMARY_OPENING_TAG);
                if (startingIndex == -1)
                    return "";

                int endingIndex = Xml.IndexOf(SUMMARY_CLOSING_TAG);
                if (endingIndex == -1)
                    return "";

                int contentStart = startingIndex + SUMMARY_OPENING_TAG.Length;
                return Xml.Substring(contentStart, endingIndex - contentStart).Trim();
            }
        }

        private readonly string TYPEPARAM_CLOSING_TAG = "</typeparam>";

        public string GetTypeParameterDescription(string typeParamName)
        {
            if (string.IsNullOrWhiteSpace(Xml))
                return "";

            string typeParamOpeningTag = string.Format("<typeparam name=\"{0}\">", typeParamName);

            int startingIndex = Xml.IndexOf(typeParamOpeningTag);
            if (startingIndex == -1)
                return "";

            int endingIndex = Xml.IndexOf(TYPEPARAM_CLOSING_TAG);
            if (endingIndex == -1)
                return "";

            int contentStart = startingIndex + typeParamOpeningTag.Length;
            return Xml.Substring(contentStart, endingIndex - contentStart).Trim();
        }
    }
}