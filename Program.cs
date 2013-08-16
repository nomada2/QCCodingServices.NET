//#define LOCAL_DEV

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.WebHost.Endpoints;
using QuantConnect.Server.Autocomplete.Models;
using QuantConnect.Server.Autocomplete.Services.ProjectModelRepository;

namespace QuantConnect.Server.Autocomplete.Worker
{
    class Program
    {
        public static readonly string NO_RESULTS_RESPONSE = "{ \"autocomplete\": { } }";

        static void Main(string[] args)
        {
            //CommandLineAutocompleteMain(args)
            AutocompleteServiceMain(args);
        }


        public class ApiError
        {
            public string FullName { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
        }

        public class Hello
        {
            public string Name { get; set; }
        }

        public class HelloResponse
        {
            public string Result { get; set; }
        }

        public class HelloService : Service
        {
            public object Any(Hello request)
            {
                return new HelloResponse { Result = "Hello, " + request.Name };
            }
        }

        public class AutoCompleteRequest
        {
            public int UserId { get; set; }
            public int ProjectId { get; set; }
            public int FileId { get; set; }
            
            public int Line { get; set; }
            public int Column { get; set; }

            public string SessionId { get; set; }


            public int iUserId { get { return UserId; } set { UserId = value; } }
            public int iProjectId { get { return ProjectId; } set { ProjectId = value; } }
            public int iFileId { get { return FileId; } set { FileId = value; } }
            public int iRow { get { return Line; } set { Line = value; } }
            public int iColumn { get { return Column; } set { Column = value; } }
        }

        public class AutoCompleteRequestService : Service
        {
            public object Any(AutoCompleteRequest request)
            {
                try
                {
                    var sb = new StringBuilder("autocomplete args: ");
                    sb.AppendFormat(" [UserId:{0}]", request.UserId);
                    sb.AppendFormat(" [ProjectId:{0}]", request.ProjectId);
                    sb.AppendFormat(" [FileId:{0}]", request.FileId);
                    sb.AppendFormat(" [Line:{0}]", request.Line);
                    sb.AppendFormat(" [Column:{0}]", request.Column);
                    Console.WriteLine(sb.ToString(), false);


                    var fileRequest = new FileOperationRequest()
                    {
                        UserId = request.UserId,
                        ProjectId = request.ProjectId,
                        FileId = request.FileId,
                        CompleteCode = new FileCodeCompletionRequest()
                        {
                            AutoComplete = true,
                            LineNumber = request.Line,
                            ColumnNumber = request.Column,
                        },
                        Parse = new FileParseRequest()
                    };

                    FileOperationResponse response = AutocompleteServiceUtil.DoAutoComplete(fileRequest);

                    return response;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString(), false);
                    
                    return new ApiError()
                    {
                        FullName = ex.GetType().FullName,
                        Message = ex.Message,
                        StackTrace = ex.StackTrace
                    };
                }
            }
        }


        //Define the Web Services AppHost
        public class AppHost : AppHostHttpListenerBase
        {
            public AppHost() : base("StarterTemplate HttpListener", typeof(HelloService).Assembly) { }

            public override void Configure(Funq.Container container)
            {
                QuantConnect.Server.Autocomplete.NRefactoryUtils.LoadReferencesInBackground();

                //SetConfig(new EndpointHostConfig()
                //{
                //    RawHttpHandlers = { httpRequest => httpRequest.}
                //});
                
                // adapted from https://github.com/ServiceStack/ServiceStack/wiki/Request-and-response-filters
                this.ResponseFilters.Add((httpReq, httpResp, dto) =>
                {
                    if (httpReq.ResponseContentType == ContentType.Json)
                    {
                        var helloResponse = dto as HelloResponse;
                        if (helloResponse != null && helloResponse.Result.ToLower().Contains("paul"))
                        {
                            httpResp.Write("{ \"Result\": \"Paul is da maaaan!!!!\" }");
                            httpResp.Close();
                        }

                        var fileOperationResponse = dto as FileOperationResponse;
                        if (fileOperationResponse != null)
                        {
                            string json = JsonConvert.SerializeObject(fileOperationResponse);
                            //string jsonResponse = string.Format("{{ \"autocomplete\": {0} }}", completionJson);
                            httpResp.Write(json);
                            httpResp.Close();
                        }

                        if (dto is ApiError)
                        {
                            //httpResp.Write(NO_RESULTS_RESPONSE);
                            string errorJson = JsonConvert.SerializeObject(dto);
                            httpResp.Write(errorJson);
                            httpResp.Close();
                        }
                    }
                });

                Routes
                    // Local test URL:  http://localhost:1337/api/autocomplete/478/125/263/9/1
                    // 
                    .Add<AutoCompleteRequest>("/api/autocomplete/{UserId}/{ProjectId}/{FileId}/{Line}/{Column}")
                    .Add<AutoCompleteRequest>("/api/autocomplete/{UserId}/{ProjectId}/{FileId}")
                    .Add<AutoCompleteRequest>("/api/autocomplete")
                    .Add<Hello>("/hello")
                    .Add<Hello>("/hello/{Name}");
            }
        }

        static void AutocompleteServiceMain(string[] args)
        {
            int port = 80; // 1337;
            if (args.Length > 0)
            {
                Int32.TryParse(args[0], out port);
            }

            //var listeningOn = args.Length == 0 ? "http://*:1337/" : args[0];
            string urlBase = string.Format("http://*:{0}/", port);
            var appHost = new AppHost();
            appHost.Init();
            appHost.Start(urlBase);

            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, urlBase);
            Console.ReadKey();
        }

        /// <summary>
        /// Needs to accept args in the following format:
        ///    {$iUserID} {$iProjectID} {$iFileID} {$iRow} {$iColumn}
        /// Example PHP command:
        ///    $sCommand = "mono /QuantConnect.Server.Autocomplete.Worker/bin/QuantConnect.Server.Autocomplete.Worker.exe {$iUserID} {$iProjectID} {$iFileID} {$iRow} {$iColumn}";
        /// 
        /// Output should be a JSON-formatted object in the following form:
        ///    { autocomplete: [ ... ] }
        /// 
        /// Input format:
        ///    UserId  ProjectId  SessionId  FileId  RowNumber  ColumnNumber
        /// Note:  RowNumber and ColumnNumber apply to the location within the file specified by FileId.
        /// Input Example:
        ///    478 125 MyArbitrarySessionIdThatCouldBeHelpfulForCachingSerializedProjectInfo 263 9 1
        ///  > Interpretation of above example:
        ///    User=Paul(478)  Project=paul-project(125)  SessionId  File=Main.cs(263)  Row=9  Column=1
        /// 
        /// An example url:
        ///    http://autocomplete.quantconnect.com/api.php?sAction=autocomplete&iUserID=478&iProjectID=125&iFileID=263&iRow=10&iColumn=123
        /// </summary>
        /// <param name="args"></param>
        static void CommandLineAutocompleteMain(string[] args)
        {
            var sb = new StringBuilder("QuantConnect.Server.Autocomplete.Worker args: ");
            for (int i = 0; i < args.Length; i++)
                sb.AppendFormat("{0} ", args[i]);
            Console.WriteLine(sb.ToString(), false);


            int userId = 0, projectId = 0, fileId = 0, line = 0, column = 0;
            string sessionId = "";
            if (args.Length < 6 ||
                !Int32.TryParse(args[0].Trim('\''), out userId) ||
                !Int32.TryParse(args[1].Trim('\''), out projectId) ||
                // session ID doesn't need processing
                !Int32.TryParse(args[3].Trim('\''), out fileId) ||
                !Int32.TryParse(args[4].Trim('\''), out line) ||
                !Int32.TryParse(args[5].Trim('\''), out column))
            {
                WriteResponse(NO_RESULTS_RESPONSE);
                return;
            }

            try
            {
                var request = new FileOperationRequest()
                {
                    UserId = userId,
                    ProjectId = projectId,
                    FileId = fileId,
                    CompleteCode = new FileCodeCompletionRequest()
                    {
                        AutoComplete = true,
                        LineNumber = line,
                        ColumnNumber = column,
                    },
                    Parse = new FileParseRequest()
                    //SyncContent = new FileSyncContentRequest()
                };

                FileOperationResponse response = AutocompleteServiceUtil.DoAutoComplete(request);

                string completionJson = JsonConvert.SerializeObject(response);
                //string jsonResponse = string.Format("{{ \"autocomplete\": {0} }}", completionJson);
                WriteResponse(completionJson);
            }
            catch (Exception ex)
            {
                WriteResponse(ex.ToString());
                Console.WriteLine(ex.ToString(), false);
            }
        }


        static void WriteResponse(string responseJson)
        {
            Console.WriteLine(responseJson);
        }
    }

    public static class AutocompleteServiceUtil
    {
        public static FileOperationResponse DoAutoComplete(FileOperationRequest request)
        {
            FileOperationResponse response = new FileOperationResponse();
            ProjectModel projectModel = null;
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                var projectModelRepo = EndpointHost.AppHost.TryResolve<IProjectModelRepository>();

                projectModel = projectModelRepo.GetProject(request.UserId, request.ProjectId);

                //#if LOCAL_DEV
                //                // Note: there's only one project in this repo:  project 478 for user 125
                //                var projectDto = MockWebServiceUtility.LoadProject(request.UserId, request.ProjectId);
                //                if (projectDto == null)
                //                    throw new Exception("Could not find project in internal repository.");

                //                projectModel = ProjectModelConverters.FromDtoToModel(projectDto);
                //#else
                //                //Get files from database matching project ID:
                //                List<SimulatorFile> lSources = QC.DB.GetProjectSource(request.UserId, request.ProjectId, true);

                //                // Fabricate a ProjectModel from the list of files
                //                projectModel = new ProjectModel()
                //                {
                //                    OwnerUserId = request.UserId,
                //                    ProjectId = request.ProjectId
                //                };
                //                foreach (SimulatorFile cFile in lSources)
                //                {
                //                    var fileModel = new ProjectFileModel()
                //                    {
                //                        ProjectId = request.ProjectId,
                //                        Id = cFile.iID,
                //                        Name = cFile.sName,
                //                        Content = cFile.sContent
                //                    };
                //                    projectModel.Children.Add(fileModel);
                //                    //aSourceCode.Add(cFile.iID.ToString() + ".cs", cFile.sContent);
                //                    //aFileID2NameMap.Add(cFile.iID, cFile.sName);
                //                }

                //#endif


                var analysisRequest = new ProjectAnalysisRequest()
                {
                    ProjectModel = projectModel,
                    CodeCompletionParameters = new ProjectAnalysisCodeCompletionParameters()
                    {
                        FileId = request.FileId,
                        Line = request.CompleteCode.LineNumber,
                        Column = request.CompleteCode.ColumnNumber,
                        Offset = request.CompleteCode.Offset,
                        CtrlSpace = true // always true for now
                    }
                };


                // Analyze!
                ProjectAnalysisResult analysisResult = NRefactoryUtils.RunFullProjectAnalysis(analysisRequest);


                // Convert analysis result model to file operation response DTO
                if (analysisResult.CompletionOptions != null)
                {
                    response.CodeCompletion = new FileCodeCompletionResponse();
                    response.CodeCompletion.CompletionOptions = analysisResult.CompletionOptions
                        .Select(CodeCompletionResultUtility.FromICompletionDataToFileCodeCompletionResult).ToArray();
                    for (int i = 0, len = response.CodeCompletion.CompletionOptions.Length; i < len; i++)
                        response.CodeCompletion.CompletionOptions[i].Id = i;
                    response.CodeCompletion.CompletionWord = analysisResult.CompletionWord;
                    if (analysisResult.BestMatchToCompletionWord != null)
                        response.CodeCompletion.BestMatchToCompletionWord = response.CodeCompletion.CompletionOptions.FirstOrDefault(x => x.CompletionText == analysisResult.BestMatchToCompletionWord.CompletionText);
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
                response.ParseResults = allErrors.ToArray();


                /*
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
                response.MsElapsed = analysisResult.TimeElapsed.TotalMilliseconds; // string.Format("{0} ms", analysisResult.TimeElapsed.TotalMilliseconds);
                */
                response.Status.Success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), false);
                response.Status.SetError(ex);
            }
            finally
            {
                //response.CodeCompletion.MsElapsed = analysisResult.TimeElapsed.TotalMilliseconds; // string.Format("{0} ms", analysisResult.TimeElapsed.TotalMilliseconds);
                response.Status.MsElapsed = sw.ElapsedMilliseconds;
            }

            return response;
        }
    }
}
