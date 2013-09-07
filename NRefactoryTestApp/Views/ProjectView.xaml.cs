using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using ServiceStack.WebHost.Endpoints;

using QuantConnect.CodingServices;
using QuantConnect.CodingServices.Models;
using QuantConnect.CodingServices.Services.ProjectModelRepository;

using NRefactoryTestApp.ViewModels;

namespace NRefactoryTestApp.Views
{
    /// <summary>
    /// Interaction logic for ProjectView.xaml
    /// </summary>
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();
            //TaskScheduler.FromCurrentSynchronizationContext
            SelectedFileContent.PreviewMouseUp += SelectedFileContentOnPreviewMouseUp;
        }

        private void SelectedFileContentOnPreviewMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            UpdateLineAndColumnOfCursorPositionInViewModel(false);
        }

        private void SelectedFileContent_KeyDown(object sender, KeyEventArgs e)
        {
            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (ctrl & e.Key == Key.Space)
            {
                e.Handled = true;
                //MessageBox.Show("Get Intellisense info...");

                // TEMPORARY...
                //bool ctrlSpace = CtrlSpaceCheckBox.IsChecked == true;
                //AnalyzeProject(ctrlSpace);

                AnalyzeProject2(true);
            }
        }

        private bool IsNonprintableCharacter(Key key)
        {
            return (
                       // includes pgup, pgdown, up, down, left, right, home, end
                   (key >= Key.PageUp && key <= Key.Down)
                   );
        }


        /// <summary>
        /// try to convert the content index to a line and column
        /// </summary>
        /// <param name="fileVm"></param>
        /// <param name="caretIndex"></param>
        private void ResolveLineAndColumnPositionInFileFromCaretIndex(ProjectFileViewModel fileVm)
        {
            int caretIndex = fileVm.CaretIndex;
            var contentParts = fileVm.Content.Split(new[] { "\r\n" }, StringSplitOptions.None);
            int newStart = 0;
            bool done = false;
            for (int line = 0; line < contentParts.Length && !done; line++)
            {
                string lineContent = contentParts[line];
                int start = newStart;
                int end = newStart + lineContent.Length;
                if (caretIndex >= start && caretIndex <= end)
                {
                    fileVm.CaretLine = line + 1;
                    fileVm.CaretColumn = caretIndex - start + 1;
                    done = true;
                }
                newStart = end + 2;
            }

            fileVm.CaretLocation = string.Format("line {0}, col {1}", fileVm.CaretLine, fileVm.CaretColumn);
        }

        private bool UpdateLineAndColumnOfCursorPositionInViewModel(bool bypassContentSynchronizationWithViewModel)
        {
            ProjectViewModel proj = DataContext as ProjectViewModel;
            if (proj == null)
                return false;

            ProjectFileViewModel projFile = proj.SelectedProjectItem as ProjectFileViewModel;
            if (projFile == null)
                return false;
            
            // Update the caret index.  I'd like to just data-bind the thing, but it's not a dependency property, 
            // and this seems to be the simplest way to do it.
            projFile.CaretIndex = SelectedFileContent.CaretIndex;

            if (bypassContentSynchronizationWithViewModel)
            {
                // Update file content
                projFile.Content = SelectedFileContent.Text;
            }

            ResolveLineAndColumnPositionInFileFromCaretIndex(projFile);

            return true;
        }


        private void SelectedFileContent_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // if input was not a control character, update the text
            if (!UpdateLineAndColumnOfCursorPositionInViewModel(IsNonprintableCharacter(e.Key)))
                return;

            // Run code completion when a period is entered
            if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
            {
                AnalyzeProject2(false);
            }

            return;
            /*
            if (_fileToParse == projFile)
            {
                // reset time
                _timeAtWhichToParse = DateTime.Now + temporalParseBuffer;
            }
            else if (_fileToParse == null)
            {
                // set file and time to parse
                _fileToParse = projFile;
                _timeAtWhichToParse = DateTime.Now + temporalParseBuffer;
                Task.Factory.StartNew(() =>
                {
                    while (_timeAtWhichToParse > DateTime.Now)
                    {
                        // If file has changed by now, just bail out.
                        // For the purposes of this prototype, it's not important to deal with this.
                        if (_fileToParse != projFile) return;

                        Thread.Sleep(_timeAtWhichToParse - DateTime.Now);
                    }
                    // at this point, the timeAtWhichToParse should have passed

                    // Check just one more time...
                    if (_fileToParse != projFile) return;

                    ParseFile();
                });
            }
            */
        }

        private readonly TimeSpan temporalParseBuffer = TimeSpan.FromMilliseconds(2000);
        private ProjectFileViewModel _fileToParse;
        private DateTime _timeAtWhichToParse;

        private void ParseFile()
        {
            if (Dispatcher != null && !Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)ParseFile);
                return;
            }

            ProjectFileViewModel fileToParse = _fileToParse;

            #region Do Parsing and other stuff...

            //Logger.AppendLine("this is where we will parse file {0}...", fileToParse.Name);
            //ParseFile(fileToParse);

            #endregion

            // if the preconditions for this method invocation still hold true, then clear the file to parse ref to indicate that we've parse the file.
            if (fileToParse == _fileToParse && DateTime.Now > _timeAtWhichToParse)
                _fileToParse = null;
        }

        //private void ParseFile(ProjectFileViewModel fileViewModelToParse)
        //{
        //    var fileDto = ProjectMVVMConverters.FromViewModelToModel(fileViewModelToParse);

        //    MockWebServiceUtility.ParseFile(fileDto, response =>
        //    {
        //        if (response.Parse == null)
        //            return;

        //        var errors = response.Parse.Errors;
        //        //response.Parse.Errors
        //        if (errors != null)
        //        {
        //            Logger.AppendLine("{0} error(s) detected.", errors.Length);
        //            if (errors.Length > 0)
        //                foreach (var err in errors)
        //                    Logger.AppendLine("{0}: In file {1} ({2}, {3}): {4}", err.Type, err.FileName, err.Line, err.Column, err.Message);
        //        }

        //    });
        //}

        //private static FileOperationResponse DoAutoComplete(FileOperationRequest request)
        //{
        //    return AutocompleteServiceUtil.DoAutoComplete(request);
        //}


        private void AnalyzeProject2(bool ctrlSpace)
        {
            //ctrlSpace = true;
            var sw = Stopwatch.StartNew();

            var projectVm = (ProjectViewModel)DataContext;
            var fileVm = projectVm.SelectedProjectItem as ProjectFileViewModel;
            if (fileVm == null)
            {
                Logger.AppendLine("A file must be selected");
                return;
            }

            fileVm.CaretIndex = SelectedFileContent.CaretIndex;
            // Synchronize the view's content with the viewmodel (since by default this is only done AFTER the textbox loses focus)
            fileVm.Content = SelectedFileContent.Text;

            // update file content in local data store if not already done
            var projectModelRepo = EndpointHost.AppHost.TryResolve<IProjectModelRepository>();
            projectModelRepo.SaveFileContent(478, fileVm.ProjectId, fileVm.Id, fileVm.Content);


            var fileRequest = new FileOperationRequest()
            {
                UserId = 478, //projectVm..UserId,
                ProjectId = projectVm.ProjectId,
                FileId = fileVm.Id,
                CompleteCode = new FileCodeCompletionRequest()
                {
                    AutoComplete = true,
                    Offset = fileVm.CaretIndex,
                    LineNumber = fileVm.CaretLine,
                    ColumnNumber = fileVm.CaretColumn,
                    CtrlSpace = ctrlSpace
                },
                Parse = new FileParseRequest()
            };


            var fileResponse = AutocompleteServiceUtil.DoAutoComplete(fileRequest);

#if false

            var projectDto = ProjectMVVMConverters.FromViewModelToModel(projectVm);

            var projectModel = ProjectModelConverters.FromDtoToModel(projectDto);

            var analysisRequest = new ProjectAnalysisRequest()
            {
                ProjectModel = projectModel,
                CodeCompletionParameters = new ProjectAnalysisCodeCompletionParameters()
                {
                    CtrlSpace = ctrlSpace,
                    FileId = fileVm.Id,
                    Offset = fileVm.CaretIndex
                    //Line = request.CompleteCode.LineNumber,
                    //Column = request.CompleteCode.ColumnNumber,
                    //CtrlSpace = true // always true for now
                }
            };

            ProjectAnalysisResult analysisResult = NRefactoryUtils.RunFullProjectAnalysis(analysisRequest);

            //StatelessProjectResponse response = MockWebServiceUtility.Server_HandleStatelessCodeCompletionRequest(request);

            FileOperationResponse response = new FileOperationResponse();
            // Convert analysis result model to file operation response DTO
            if (analysisResult.CompletionOptions != null)
            {
                response.CodeCompletion = new FileCodeCompletionResponse();
                response.CodeCompletion.CompletionOptions = analysisResult.CompletionOptions
                    .Select(ProjectModelConverters.FromICompletionDataToFileCodeCompletionResult).ToArray();
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
#endif

            //var jsonResponse = JsonConvert.SerializeObject(fileResponse, Formatting.Indented);
            //Logger.AppendLine(jsonResponse);

            // Summarize results...
            Logger.AppendLine("=========================================================");
            Logger.AppendLine("Project analysis completed in {0} ms", sw.ElapsedMilliseconds);

            if (fileResponse.CodeCompletion == null)
            {
                Logger.AppendLine("No Completion Results.");
                Logger.SetCodeCompletionOptions(null, null);
            }
            else
            {
                var codeCompletion = fileResponse.CodeCompletion;

                Logger.SetCodeCompletionOptions(codeCompletion.CompletionOptions, codeCompletion.BestMatchToCompletionWord);

                Logger.AppendLine("Completion Results...");
                Logger.AppendLine("  Input:  Line:{0}  Col:{1}  Offset:{2}", codeCompletion.Line, codeCompletion.Column, codeCompletion.Offset);
                Logger.AppendLine("  Context: \"{0}\" <cursor> \"{1}\"", codeCompletion.TextBeforeCursor, codeCompletion.TextAfterCursor);
                Logger.AppendLine("  {0} code completion option(s) generated.", codeCompletion.CompletionOptions.Length);

                // Try to find closest matching completion result
                if (string.IsNullOrWhiteSpace(codeCompletion.CompletionWord))
                {
                    Logger.AppendLine("  No code completion word detected.");
                }
                else
                {
                    if (codeCompletion.BestMatchToCompletionWord != null)
                    {
                        Logger.AppendLine("  Detected code completion word, \"{0}\", most closely matches completion option \"{1}\".",
                                          codeCompletion.CompletionWord, codeCompletion.BestMatchToCompletionWord.CompletionText);
                    }
                    else
                    {
                        Logger.AppendLine("  Detected code completion word: {0}", codeCompletion.CompletionWord);
                    }
                }
            }


            if (fileResponse.ParseResults != null)
            {
                Logger.AppendLine("{0} error(s) detected.", fileResponse.ParseResults.Length);
                if (fileResponse.ParseResults.Length > 0)
                    foreach (var err in fileResponse.ParseResults)
                        Logger.AppendLine("{0}: In file {1} ({2}, {3}): {4}", err.Type, err.FileName, err.Line, err.Column, err.Message);
            }

        }



        private void AnalyzeProject(bool ctrlSpace)
        {
            ctrlSpace = true;
            var sw = Stopwatch.StartNew();

            var projectVm = (ProjectViewModel)DataContext;
            var fileVm = projectVm.SelectedProjectItem as ProjectFileViewModel;
            if (fileVm == null)
            {
                Logger.AppendLine("A file must be selected");
                return;
            }
            fileVm.CaretIndex = SelectedFileContent.CaretIndex;

            // Synchronize the view's content with the viewmodel (since by default this is only done AFTER the textbox loses focus)
            fileVm.Content = SelectedFileContent.Text;

            var projectDto = ProjectMVVMConverters.FromViewModelToModel(projectVm);
            var request = new StatelessProjectRequest()
            {
                Project = projectDto,
                CodeCompletionParameters = new StatelessProjectCodeCompletionParameters()
                {
                    CtrlSpace = ctrlSpace,
                    FileId = fileVm.Id,
                    Offset = fileVm.CaretIndex
                }
            };

            StatelessProjectResponse response = MockWebServiceUtility.Server_HandleStatelessCodeCompletionRequest(request);

            var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            //Logger.AppendLine(jsonResponse);

            // Summarize results...
            Logger.AppendLine("Project analysis completed in {0} ms", sw.ElapsedMilliseconds);

            if (response.CompletionOptions != null)
            {
                // Order results by completion text
                response.CompletionOptions = response.CompletionOptions.OrderBy(x => x.CompletionText).ToArray();
                Logger.AppendLine("{0} code completion option(s) generated.", response.CompletionOptions.Length);

                // Try to find closest matching completion result
                if (string.IsNullOrWhiteSpace(response.CompletionWord))
                {
                    Logger.AppendLine("No code completion word detected.");
                }
                else
                {
                    //response. .CompletionOptionMostCloselyMatchingCompletionWord = response.CompletionOptions
                    //    .FirstOrDefault(x => x.CompletionText.CompareTo(response.CompletionWord) >= 0);
                    //response.CompletionOptionMostCloselyMatchingCompletionWord = response.CompletionOptions.FirstOrDefault(x => x.CompletionText.StartsWith(response.CompletionWord, StringComparison.InvariantCultureIgnoreCase));
                    if (response.BestMatchToCompletionWord != null)
                    {
                        Logger.AppendLine("Detected code completion word, \"{0}\", most closely matches completion option \"{1}\".",
                                          response.CompletionWord, response.BestMatchToCompletionWord.CompletionText);
                    }
                    else
                    {
                        Logger.AppendLine("Detected code completion word: {0}", response.CompletionWord);
                    }
                }

            }

            Logger.SetCodeCompletionOptions(response.CompletionOptions, response.BestMatchToCompletionWord); // response.CompletionWord);

            if (response.Errors != null)
            {
                Logger.AppendLine("{0} error(s) detected.", response.Errors.Length);
                if (response.Errors.Length > 0)
                    foreach (var err in response.Errors)
                        Logger.AppendLine("{0}: In file {1} ({2}, {3}): {4}", err.Type, err.FileName, err.Line, err.Column, err.Message);
            }

        }

        //private void AppendToProjectLog(string format, params object[] args)
        //{
        //    var str = string.Format(format, args);
        //    ProjectLogTextBox.AppendText(str);
        //    if (!str.EndsWith("\r\n"))
        //        ProjectLogTextBox.AppendText("\r\n");
        //}


    }

    public class ProjectDataTemplateSelector : DataTemplateSelector
    {
        //public NamedItemBrowserDataTemplateSelector()
        //{
        //}

        public DataTemplate FileDataTemplate { get; set; }
        public DataTemplate DirectoryDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ProjectFileViewModel)
                return FileDataTemplate;
            else if (item is ProjectDirectoryViewModel)
                return DirectoryDataTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
