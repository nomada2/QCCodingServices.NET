using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qc.server.autocomplete.models
{
    public interface IUserContext
    {
        int UserId { get; set; }
    }

    public interface IProjectContext : IUserContext
    {
        int ProjectId { get; set; }
    }

    public interface IFileContext : IProjectContext
    {
        int FileId { get; set; }
    }

    public enum ResponseStatus
    {
        /// <summary>
        /// This is purely a default state.  
        /// No response should ever be sent with this status.
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// A basic "success" / no error status
        /// </summary>
        OK = 1,

        /// <summary>
        /// A basic "error" status
        /// </summary>
        Error = 2,

    }

    public interface IResponse
    {
        /// <summary>
        /// Indicates the status of the response
        /// </summary>
        ResponseStatus Status { get; set; }

        /// <summary>
        /// An optional message providing additional information beyond the status indicator.
        /// </summary>
        string Message { get; set; }
    }

    public abstract class BasicResponse : IResponse
    {
        public BasicResponse()
        {
            // even though this is the default, set it explicitly for clarity
            Status = ResponseStatus.NotSet;
            // Set to an empty message for uniformity (i.e. so we don't end up with null messages)
            Message = "";
        }
        public ResponseStatus Status { get; set; }
        public string Message { get; set; }

        public void SetError(string errorMessage)
        {
            Status = ResponseStatus.Error;
            Message = errorMessage;
        }

        public void SetError(Exception exception)
        {
            Status = ResponseStatus.Error;
            Message = exception.ToString();
        }
    }


    // http://james.newtonking.com/projects/json/help/html/CustomCreationConverter.htm
    // http://stackoverflow.com/questions/13067842/json-net-deserialize-nested-arrays-into-strongly-typed-object

    public class FileOperationRequest : IFileContext
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int FileId { get; set; }

        // NOTE:  If any of the following are set to null, it will be interpreted by the server as not requested
        
        public FileSyncContentRequest SyncContent { get; set; }

        public FileParseRequest Parse { get; set; }

        public FileCodeCompletionRequest CompleteCode { get; set; }

    }

    public class FileOperationResponse : BasicResponse, IFileContext
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int FileId { get; set; }

        // NOTE:  If any of the following are set to null, they should be interpreted by the client as not requested

        public FileSyncContentResponse SyncContent { get; set; }

        public FileParseResponse Parse { get; set; }

        public FileCodeCompletionResponse CompleteCode { get; set; }

    }

    #region File Parsing

    public class FileSyncContentRequest
    {
        public string Content { get; set; }
    }

    public class FileSyncContentResponse : BasicResponse
    {
        //public DateTime LastModified { get; set; }
    }

    #endregion
    

    #region File Parsing

    public class FileParseRequest
    {
    }

    public class FileParseResult
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public ICSharpCode.NRefactory.TypeSystem.ErrorType Type { get; set; }
        public string Message { get; set; }
    }

    public class FileParseResponse : BasicResponse
    {
        /// <summary>
        /// This set of items actually constitutes both Errors and Warnings
        /// </summary>
        public FileParseResult[] Errors { get; set; }
        //public FileParseResult[] Warnings { get; set; }
    }

    #endregion


    #region Code Completion

    public class FileCodeCompletionRequest
    {
        public int Offset { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public bool AutoComplete { get; set; }
    }

    public class FileCodeCompletionResult
    {
        /// <summary>
        /// Candidate text to be used in the document (for insertion/replacement)
        /// </summary>
        public string CompletionText { get; set; }
        /// <summary>
        /// Display text to represent the code completion option in the code completion list
        /// </summary>
        public string DisplayText { get; set; }
        /// <summary>
        /// (optional) A [more comprehensive] description of the code completion option, intended
        /// to be used for tooltip content.  This could contain information about the return type of the member,
        /// the type in which the member resides, domentation about the member, or anything else...
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Indicates the number of overloads which exist for the completion option.
        /// NOTE: ONLY applies to C# completion options for which overloads can exist (i.e. methods)
        /// </summary>
        public int OverloadCount { get; set; }
        /// <summary>
        /// The category of the option
        /// </summary>
        public string CompletionCategoryDisplayText { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]  {1}  {2}\"{3}\"", CompletionCategoryDisplayText, DisplayText, 
                (OverloadCount == 0 ? "" : "("+OverloadCount+" overloads)  "), Description);
        }
    }

    public class FileCodeCompletionResponse : BasicResponse
    {
        public FileCodeCompletionResult[] CompletionOptions { get; set; }
    }

    #endregion


    #region Stateless Project Operations

    public class StatelessProjectRequest
    {
        public ProjectDto Project { get; set; }
        //public StatelessProjectParseParameters Parse { get; set; }
        public StatelessProjectCodeCompletionParameters CodeCompletionParameters { get; set; }
    }

    public class StatelessProjectCodeCompletionParameters
    {
        public bool CtrlSpace { get; set; }
        public int FileId { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int Offset { get; set; }
    }

    public class StatelessProjectResponse
    {
        public string TimeElapsed { get; set; }
        public FileParseResult[] Errors { get; set; }
        public FileCodeCompletionResult[] CompletionOptions { get; set; }
        public string CompletionWord { get; set; }
        public FileCodeCompletionResult CompletionOptionMostCloselyMatchingCompletionWord { get; set; }
    }

    #endregion
}
