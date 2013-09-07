using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using qc.server.autocomplete.models;

namespace qc.server.autocomplete
{
    public static class MockWebServiceUtility
    {
        public static ProjectDto LoadProject(int userId, int projectId)
        {
            var projectModel = ServerProjectRepository.GetProject(userId, projectId);
            var projectDto = ProjectModelConverters.FromModelToDto(projectModel);
            return projectDto;
        }

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
                    .Select(x => new FileCodeCompletionResult()
                    {
                        CompletionText = x.CompletionText,
                        DisplayText = x.DisplayText,
                        Description = x.Description,
                        OverloadCount = x.OverloadedData.Count(),
                        CompletionCategoryDisplayText = (x.CompletionCategory == null ? "" : x.CompletionCategory.DisplayText)
                    }).ToArray();
                response.CompletionWord = analysisResult.CompletionWord;
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
            response.TimeElapsed = string.Format("{0} ms", analysisResult.TimeElapsed.TotalMilliseconds);

            return response;
        }



        public static class ServerProjectRepository
        {
            /// <summary>
            /// Yeah -- So I know there's only a single project in this array of projects.
            /// It may not seem very useful now, but this should be good enough to prove the idea...
            /// </summary>
            private static ProjectModel[] projects = new ProjectModel[]
            {
                new ProjectModel()
                {
                    OwnerUserId = 478,
                    ProjectId = 125,
                    Name = "Project X",
                    Children = new List<IProjectItemModel>()
                    {
                        new ProjectDirectoryModel()
                        {
                            ProjectId = 125,
                            Id = 262,
                            Name = "Math",
                            Children = new List<IProjectItemModel>()
                            {
                                new ProjectFileModel()
                                {
                                    ProjectId = 125,
                                    Id = 264,
                                    Name = "RollingAverage.cs",
                                    Content = "/// <summary>\r\n///    Basic Template v0.1 :: Rolling Average\r\n/// </summary>    \r\nusing System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\n\r\nnamespace qc {\r\n\r\n    /// <summary>\r\n    /// Example Helper Class: Basic Math Routines.\r\n    /// Using the QCS you can create subfolders, classes. \r\n    /// All the code is compiled into your algorithm.\r\n    /// </summary>    \r\n    public partial class MathAverage {\r\n\r\n        public int iSamplePeriod = 10;\r\n        public decimal dCurrentAverage = 0;\r\n\r\n        /// <summary>\r\n        /// Example helper class: Add a new sample to a rolling average.\r\n        /// </summary>\r\n        /// <param name=\"dNewSample\">Decimal new sample value</param>\r\n        /// <returns>decimal current rolling average.</returns>\r\n        public static decimal RollingAverage(decimal dNewSample) {\r\n\r\n            Random cRand = new Random();\r\n            return dNewSample * ((decimal)cRand.NextDouble());\r\n        \r\n        }\r\n\r\n    }\r\n}"
                                }
                            }
                        },
                        new ProjectFileModel()
                        {
                            ProjectId = 125,
                            Id = 263,
                            Name = "Main.cs",
                            Content = "/// \r\n///    Basic Template v0.1\r\n///     \r\nusing System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\n\r\nnamespace qc {\r\n\r\n    ///\r\n    /// Your Algorithm Class Name:\r\n    /// This can be anything you like but it must extend the QCAlgorithm Class.\r\n    ///     \r\n    public partial class BasicTemplateAlgorithm : QCAlgorithm {\r\n    \r\n        /// \r\n        /// Initialize the data and resolution you require for your strategy:\r\n        /// -> AddSecurity(MarketType.Equity, string Symbol, Resolution)\r\n        ///    MarketType.Equity is the only type supported for now.\r\n        ///    Resolution can be: Resolution.Tick, .Second or .Minute\r\n        /// -> Use this section to set up your algorithm.\r\n        /// \r\n        public override void Initialize() {\r\n            AddSecurity(MarketType.Equity, \"AAPL\", Resolution.Second); \r\n        }\r\n\r\n        /// \r\n        /// Handle a New TradeBar Event\r\n        ///    -> This event is fired to recieve new data from Minute or Second events.\r\n        /// -> Data arrives in a Dictionary, indexed by the stock symbol.\r\n        /// -> The TradeBar Class contains the properties dHigh, dLow, dOpen, dClose (\"dPrice alias\"), iVolume and dtTime\r\n        //  -> You can only have one resolution per symbol, i.e. Apple Tick or Second, but not both.\r\n        /// \r\n        /// Dictionary of TradeBar Class Data Packets\r\n        public override void OnTradeBar(Dictionary<string, TradeBar> lData) {\r\n            if (Portfolio.bHoldStock == false) {\r\n                Order(\"AAPL\", 10, OrderType.Market);\r\n            }\r\n        }\r\n\r\n\r\n        /// \r\n        /// Handle a New Tick Event\r\n        ///    -> Tick events are almost identical to tradebars but arrive one by one, for all stocks, through this function.\r\n        /// -> You probably want to check cTick.sSymbol to see which symbol the tick belongs to.\r\n        /// -> Tick Data Class has properties: eTickType (TickType.Trade or .Quote), DateTime dtTime of tick\r\n        //    -> If a trade tick: has the properties: decimal dPrice, int iQuantity\r\n        //  -> If a quote tick\" has the properties: decimal dBidPrice, dAskPrice, int iAskQuantity, iBidQuantity, string sBidMarket, sAskMarket \r\n        /// \r\n        /// \r\n        public override void OnTick(Tick cTick) {\r\n            if (Portfolio.bHoldStock == false) {\r\n                Order(\"AAPL\", 10, OrderType.Market);\r\n            }\r\n        }\r\n    }\r\n}"
                        },
                        new ProjectFileModel()
                        {
                            ProjectId = 125,
                            Id = 265,
                            Name = "BrowsableAttributeTest.cs",
                            Content = "using System;\r\nusing System.ComponentModel;\r\npublic class FooBar\r\n{\r\n    [EditorBrowsable(EditorBrowsableState.Always)]\r\n    public int BrowsableTest { get; set; }\r\n\r\n    [EditorBrowsable(EditorBrowsableState.Never)]\r\n    public int NotBrowsableTest { get; set; }\r\n\r\n    public void DoSomethingPointless(int i)\r\n    {\r\n        NotBrowsableTest = i;\r\n        if (i > 0)\r\n        {\r\n            NotBrowsableTest --;\r\n        	(new FooBar()).DoSomethingPointless(NotBrowsableTest-1);\r\n            DoSomethingPointless(NotBrowsableTest);\r\n        }\r\n    }\r\n}\r\n"
                        }
                    }
                }
            };

            public static ProjectModel GetProject(int userId, int projectId)
            {
                return projects.FirstOrDefault(x => x.OwnerUserId == userId && x.ProjectId == projectId);
            }

            public static ProjectFileModel GetFile(int userId, int projectId, int fileId)
            {
                var project = GetProject(userId, projectId);
                if (project == null)
                    return null;
                
                var file = project.FindFile(fileId);
                return file;
            }

        }
    }
}
