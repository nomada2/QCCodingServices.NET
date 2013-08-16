using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Completion;
using Newtonsoft.Json;

namespace QuantConnect.Server.Autocomplete.Models
{
    public class ProjectAnalysisRequest
    {
        public ProjectModel ProjectModel { get; set; }
        public ProjectAnalysisCodeCompletionParameters CodeCompletionParameters { get; set; }
    }

    public class ProjectAnalysisCodeCompletionParameters
    {
        public bool CtrlSpace { get; set; }
        public int FileId { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        /// <summary>
        /// This is an alternative way to set the position within the file.
        /// This will be used if Line and Column are both set to 0.
        /// </summary>
        public int Offset { get; set; }
    }

    public class ProjectAnalysisResult
    {
        public TimeSpan TimeElapsed { get; set; }

        #region Parse Results
        /// <summary>
        /// Errors (and Warnings) for all files in the project
        /// </summary>
        public ICSharpCode.NRefactory.TypeSystem.Error[] Errors { get; set; }
        #endregion

        #region Code Completion Results

        public int Line { get; set; }
        public int Column { get; set; }
        public int Offset { get; set; }

        public ICompletionData[] CompletionOptions { get; set; }

        public bool AutoCompleteEmptyMatch { get; set; }

        public bool AutoSelect { get; set; }

        public string DefaultCompletionString { get; set; }

        public string CompletionWord { get; set; }

        public ICompletionData BestMatchToCompletionWord { get; set; }
        
        #endregion

        //public FileParseResult[] Errors { get; set; }
        //public FileCodeCompletionResult[] CompletionOptions { get; set; }
    }



}
