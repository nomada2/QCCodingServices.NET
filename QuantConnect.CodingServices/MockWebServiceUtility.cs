using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuantConnect.CodingServices.Models;

namespace QuantConnect.CodingServices
{
    public static class MockWebServiceUtility
    {
        //public static ProjectDto LoadProject(int userId, int projectId)
        //{
        //    var projectModel = ServerProjectRepository.GetProject(userId, projectId);
        //    var projectDto = ProjectModelConverters.FromModelToDto(projectModel);
        //    return projectDto;
        //}

        /*
        public static void ParseFile(ProjectFileDto fileModel, Action<FileOperationResponse> callback)
        {
            FileOperationRequest request = new FileOperationRequest();
            
            // Set contextual identifiers
            request.UserId = 478;  // hard-coded, for now
            request.ProjectId = fileModel.ProjectId;
            request.FileId = fileModel.Id;

            // Specify operations to perform
            request.SyncContent = new FileSyncContentRequest() { Content = fileModel.Content };
            request.Parse = new FileParseRequest() { };

            Task.Factory.StartNew(() =>
            {
                var requestJson = JsonConvert.SerializeObject(request);

                #region Simulation of call to server
                var responseJson = Server_HandleStatefulFileOperationRequest(requestJson);
                #endregion

                var response = JsonConvert.DeserializeObject<FileOperationResponse>(responseJson);

                callback.Invoke(response);
            });
        }

        public static string Server_HandleStatefulFileOperationRequest(string requestJson)
        {
            var request = JsonConvert.DeserializeObject<FileOperationRequest>(requestJson);

            var response = Server_HandleStatefulFileOperationRequest(request);

            var responseJson = JsonConvert.SerializeObject(response);
            return responseJson;
        }

        private static FileOperationResponse Server_HandleStatefulFileOperationRequest(FileOperationRequest request)
        {
            FileOperationResponse response = new FileOperationResponse();

            ProjectModel projectModel = null;
            ProjectFileModel fileModel = null;

            try
            {
                projectModel = ServerProjectRepository.GetProject(request.UserId, request.ProjectId);
                if (projectModel == null)
                {
                    response.SetError("Specified project could not be located.");
                    return response;
                }

                fileModel = ServerProjectRepository.GetFile(request.UserId, request.ProjectId, request.FileId);
                if (fileModel == null)
                {
                    response.SetError("Specified file could not be located within the project.");
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.SyncContent.SetError(ex);
            }


            // Set contextual identifiers on response
            response.UserId = request.UserId;
            response.ProjectId = request.ProjectId;
            response.FileId = request.FileId;

            // Now, handle each requested operation...

            if (request.SyncContent != null)
            {
                response.SyncContent = new FileSyncContentResponse();

                fileModel.Content = request.SyncContent.Content;
                response.SyncContent.Status = ResponseStatus.OK;
            }


            if (request.Parse != null)
            {
                response.Parse = new FileParseResponse();

                try
                {
                    response.Parse.Errors = NRefactoryUtils.ParseFile(fileModel);
                    response.Parse.Status = ResponseStatus.OK;
                }
                catch (Exception ex)
                {
                    response.Parse.SetError(ex);
                }
            }


            if (request.CompleteCode != null)
            {
                response.CompleteCode = new FileCodeCompletionResponse();

                try
                {
                    var analysisRequest = new ProjectAnalysisRequest()
                    {
                        ProjectModel = projectModel,
                        CodeCompletionParameters = new ProjectAnalysisCodeCompletionParameters()
                        {
                            FileId = request.FileId,
                            Line = request.CompleteCode.LineNumber,
                            Column = request.CompleteCode.ColumnNumber
                        }
                    };

                    var analysisResult = NRefactoryUtils.RunFullProjectAnalysis(analysisRequest);
                    
                    response.CompleteCode.CompletionOptions = analysisResult.CompletionOptions.Select(x => new FileCodeCompletionResult()
                    {
                        CompletionText = x.CompletionText,
                        DisplayText = x.DisplayText
                    }).ToArray();
                    response.CompleteCode.Status = ResponseStatus.OK;
                }
                catch (Exception ex)
                {
                    response.CompleteCode.SetError(ex);
                }
            }

            response.Status = ResponseStatus.OK;
            return response;
        }
        */



        public static string Server_HandleStatelessCodeCompletionRequest(string requestJson)
        {
            var request = JsonConvert.DeserializeObject<StatelessProjectRequest>(requestJson);

            var response = Server_HandleStatelessCodeCompletionRequest(request);

            var responseJson = JsonConvert.SerializeObject(response);
            return responseJson;
        }

        public static StatelessProjectResponse Server_HandleStatelessCodeCompletionRequest(StatelessProjectRequest request)
        {
            // Convert web request model to internal analysis model
            ProjectAnalysisRequest analysisRequest = new ProjectAnalysisRequest();
            analysisRequest.ProjectModel = ProjectModelConverters.FromDtoToModel(request.Project);
            if (request.CodeCompletionParameters != null)
            {
                analysisRequest.CodeCompletionParameters = new ProjectAnalysisCodeCompletionParameters()
                {
                    CtrlSpace = request.CodeCompletionParameters.CtrlSpace,
                    Offset = request.CodeCompletionParameters.Offset,
                    FileId = request.CodeCompletionParameters.FileId,
                    Line = request.CodeCompletionParameters.Line,
                    Column = request.CodeCompletionParameters.Column
                };
            }

            // Analyze!
            ProjectAnalysisResult analysisResult = NRefactoryUtils.RunFullProjectAnalysis(analysisRequest);

            // Convert analysis result model to web service model
            StatelessProjectResponse response = new StatelessProjectResponse();
            if (analysisResult.CompletionOptions != null)
            {
                response.CompletionOptions = analysisResult.CompletionOptions
                    .Select(CodeCompletionResultUtility.FromICompletionDataToFileCodeCompletionResult)
                    .ToArray();
                for (int i = 0, len = response.CompletionOptions.Length; i < len; i++)
                    response.CompletionOptions[i].Id = i;
                response.CompletionWord = analysisResult.CompletionWord;
                if (analysisResult.BestMatchToCompletionWord != null)
                    response.BestMatchToCompletionWord = response.CompletionOptions.FirstOrDefault(x => x.CompletionText == analysisResult.BestMatchToCompletionWord.CompletionText);
            }
            var allErrors = new List<FileParseResult>();
            foreach (var fileModel in analysisRequest.ProjectModel.GetFileDescendants())
            {
                allErrors.AddRange(fileModel.Parser.ErrorsAndWarnings
                                       .Select(x => new FileParseResult()
                                       {
                                           FileId = fileModel.Id,
                                           FileName = fileModel.Name,
                                           Line = x.Region.BeginLine,
                                           Column = x.Region.BeginColumn,
                                           Type = x.ErrorType,
                                           Message = x.Message
                                       }).ToArray());
            }
            response.Errors = allErrors.ToArray();
            response.MsElapsed = analysisResult.TimeElapsed.TotalMilliseconds; // string.Format("{0}", analysisResult.TimeElapsed.TotalMilliseconds);

            return response;
        }
    }
}
