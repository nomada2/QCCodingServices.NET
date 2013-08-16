using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace QuantConnect.Server.Autocomplete.Models
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

        [JsonIgnore]
        public ResponseStatus Status { get; set; }
        
        [JsonProperty("success")]
        public bool? Success
        {
            get 
            { 
                switch(Status)
                {
                    case ResponseStatus.OK:
                        return true;
                    case ResponseStatus.Error:
                        return false;
                    case ResponseStatus.NotSet:
                        return null;
                }
                return null;
            }
        }

        [JsonProperty("message")]
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


    public class GenericResponseStatus
    {
        //public GenericResponseStatus()
        //{
        //    Success = true;
        //}

        [JsonProperty("dMsElapsed")]
        public double MsElapsed { get; set; }

        [JsonProperty("bSuccess")]
        public bool? Success { get; set; }

        [JsonProperty("sMessage")]
        public string Message { get; set; }

        public void SetError(string errorMessage)
        {
            Success = false;
            Message = errorMessage;
        }

        public void SetError(Exception exception)
        {
            Success = false;
            Message = exception.ToString();
        }
    }

    public class FileOperationResponse //: BasicResponse, IFileContext
    {
        [JsonIgnore]
        public int UserId { get; set; }
        [JsonIgnore]
        public int ProjectId { get; set; }
        [JsonIgnore]
        public int FileId { get; set; }

        public FileOperationResponse()
        {
            Status = new GenericResponseStatus();
        }
        // NOTE:  If any of the following are set to null, they should be interpreted by the client as not requested

        //[JsonIgnore]
        //public FileSyncContentResponse SyncContent { get; set; }

        /// <summary>
        /// This set of items actually constitutes both Errors and Warnings
        /// </summary>
        [JsonProperty("parseResults")]
        public FileParseResult[] ParseResults { get; set; }

        [JsonProperty("autocomplete")]
        public FileCodeCompletionResponse CodeCompletion { get; set; }

        [JsonProperty("status")]
        public GenericResponseStatus Status { get; set; }
    }

    #region File Parsing

    public class FileSyncContentRequest
    {
        public string Content { get; set; }
    }

    public class FileSyncContentResponse //: BasicResponse
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
        [JsonProperty("iFileId")]
        public int FileId { get; set; }

        [JsonProperty("sFileName")]
        public string FileName { get; set; }

        [JsonProperty("iLine")]
        public int Line { get; set; }

        [JsonProperty("iColumn")]
        public int Column { get; set; }

        [JsonProperty("sType")]
        public ICSharpCode.NRefactory.TypeSystem.ErrorType Type { get; set; }

        [JsonProperty("sMessage")]
        public string Message { get; set; }

        [JsonProperty("sDocumentContext")]
        public string DocumentContext { get; set; }
    }

    public class FileParseResponse //: BasicResponse
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

    public class FileCodeCompletionResponse //: BasicResponse
    {
        [JsonProperty("options")]
        public CodeCompletionResult[] CompletionOptions { get; set; }

        [JsonProperty("completionWord")]
        public string CompletionWord { get; set; }

        [JsonProperty("bestMatchToCompletionWord")]
        public CodeCompletionResult BestMatchToCompletionWord { get; set; }

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
        [JsonProperty("msElapsed")]
        public double MsElapsed { get; set; }

        [JsonIgnore]
        //[JsonProperty("errors")]
        public FileParseResult[] Errors { get; set; }

        [JsonProperty("options")]
        public CodeCompletionResult[] CompletionOptions { get; set; }

        [JsonProperty("completionWord")]
        public string CompletionWord { get; set; }

        [JsonProperty("bestMatchToCompletionWord")]
        public CodeCompletionResult BestMatchToCompletionWord { get; set; }
    }

    #endregion
}
