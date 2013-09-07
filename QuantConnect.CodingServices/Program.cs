//#define PRODUCTION_BUILD

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using QuantConnect.CodingServices.Models;
using QuantConnect.CodingServices.Services.ProjectModelRepository;
using ServiceStack.Common.Web;
using ServiceStack.Logging;
using ServiceStack.ServiceInterface;
using ServiceStack.WebHost.Endpoints;
//using QuantConnect.Logging;

namespace QuantConnect.CodingServices
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                    log.Info(sb.ToString());

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
                            CtrlSpace = true  // always true for now
                        },
                        Parse = new FileParseRequest()
                    };

                    FileOperationResponse response = AutocompleteServiceUtil.DoAutoComplete(fileRequest);

                    return response;
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString(), ex);
                    
                    return new ApiError()
                    {
                        FullName = ex.GetType().FullName,
                        Message = ex.Message,
                        StackTrace = ex.StackTrace
                    };
                }
            }
        }

#if PRODUCTION_BUILD
        private static void KickOffWatchdogThread()
        {
            var watchdogResetThread = new Thread((ThreadStart)delegate
            {
                Console.WriteLine("Watchdog reset thread started.");
                while (true)
                {
                    //Reset the watchdog:
                    Watchdog.Reset();

                    Thread.Sleep(10000);
                }
            });
            watchdogResetThread.Start();
        }
#endif

        // Define the Web Services AppHost
        public class AppHost : AppHostHttpListenerBase
        {
            private bool useInMemoryDataStore = false;

            public AppHost(int port) : base("StarterTemplate HttpListener", typeof(HelloService).Assembly)
            {
                // If listening on port 1337, use the in-memory data store
                if (port == 1337)
                    useInMemoryDataStore = true;
            }

            public override void Configure(Funq.Container container)
            {
                NRefactoryUtils.LoadReferencesInBackground();

                if (useInMemoryDataStore)
                    container.RegisterAutoWiredAs<InMemoryProjectModelRepo, IProjectModelRepository>();
                //else
                //    container.RegisterAutoWiredAs<PersistedProjectModelRepo, IProjectModelRepository>();

                // overkill for now...
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
                    // Local test URL:  http://localhost:1337/api/autocomplete/478/174/424/21/1
                    // OR:  http://localhost:1337/api/autocomplete?iuserid=478&iprojectid=174&ifileid=423&irow=21&icolumn=1&format=json
                    // OR:  http://localhost:1337/api/autocomplete?iuserid=478&iprojectid=174&ifileid=423&irow=36&icolumn=1&format=json
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
            // By default, service will listen on port 80.
            // However, if an argument is provided on the command line, we'll attempt to parse the first arg as an integer 
            //   and treat that as the port number to listen on.
            int port = 80;
            if (args.Length > 0)
            {
                Int32.TryParse(args[0], out port);
            }

            //var listeningOn = args.Length == 0 ? "http://*:1337/" : args[0];
            string urlBase = string.Format("http://*:{0}/", port);
            var appHost = new AppHost(port);
            appHost.Init();
            appHost.Start(urlBase);

#if PRODUCTION_BUILD
            KickOffWatchdogThread();
#endif

            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, urlBase);
            Console.ReadKey();
            Console.WriteLine("Process exiting.  Thank-you for playing!");
        }



#if false
        /// <summary>
        /// Needs to accept args in the following format:
        ///    {$iUserID} {$iProjectID} {$iFileID} {$iRow} {$iColumn}
        /// Example PHP command:
        ///    $sCommand = "mono /QuantConnect.Server.Autocomplete.worker/bin/QuantConnect.Server.Autocomplete.worker.exe {$iUserID} {$iProjectID} {$iFileID} {$iRow} {$iColumn}";
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
            var sb = new StringBuilder("QuantConnect.Server.Autocomplete.worker args: ");
            for (int i = 0; i < args.Length; i++)
                sb.AppendFormat("{0} ", args[i]);
            Log.Trace(sb.ToString());


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
                        CtrlSpace = true  // always true for now
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
                Log.Error(ex.ToString());
            }
        }


        static void WriteResponse(string responseJson)
        {
            Console.WriteLine(responseJson);
        }
#endif
    }

    public static class AutocompleteServiceUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static FileOperationResponse DoAutoComplete(FileOperationRequest request)
        {
            FileOperationResponse response = new FileOperationResponse();
            ProjectModel projectModel = null;
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                var projectModelRepo = EndpointHost.AppHost.TryResolve<IProjectModelRepository>();

                projectModel = projectModelRepo.GetProject(request.UserId, request.ProjectId);

                var analysisRequest = new ProjectAnalysisRequest()
                {
                    ProjectModel = projectModel,
                    CodeCompletionParameters = new ProjectAnalysisCodeCompletionParameters()
                    {
                        FileId = request.FileId,
                        Line = request.CompleteCode.LineNumber,
                        Column = request.CompleteCode.ColumnNumber,
                        Offset = request.CompleteCode.Offset,
                        CtrlSpace = request.CompleteCode.CtrlSpace
                    }
                };


                // Analyze!
                ProjectAnalysisResult analysisResult = NRefactoryUtils.RunFullProjectAnalysis(analysisRequest);


                // Convert analysis result model to file operation response DTO
                if (analysisResult.CompletionOptions != null)
                {
                    var codeCompletion = response.CodeCompletion = new FileCodeCompletionResponse();
                    codeCompletion.CompletionOptions = analysisResult.CompletionOptions
                        .Select(CodeCompletionResultUtility.FromICompletionDataToFileCodeCompletionResult).ToArray();
                    for (int i = 0, len = codeCompletion.CompletionOptions.Length; i < len; i++)
                        codeCompletion.CompletionOptions[i].Id = i;
                    codeCompletion.CompletionWord = analysisResult.CompletionWord;
                    if (analysisResult.BestMatchToCompletionWord != null)
                        codeCompletion.BestMatchToCompletionWord = codeCompletion.CompletionOptions.FirstOrDefault(x => x.CompletionText == analysisResult.BestMatchToCompletionWord.CompletionText);
                    // Record text around cursor
                    codeCompletion.TextBeforeCursor = analysisResult.CompletionContextBefore;
                    codeCompletion.TextAfterCursor = analysisResult.CompletionContextAfter;
                    // Record inputs
                    codeCompletion.Line = analysisResult.Line;
                    codeCompletion.Column = analysisResult.Column;
                    codeCompletion.Offset = analysisResult.Offset;
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
                log.Error(ex.ToString(), ex);
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
