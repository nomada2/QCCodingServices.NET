using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using QuantConnect.CodingServices.Models;

//using NRefactoryTestApp.models;

namespace NRefactoryTestApp
{
    // This is a tremendous hack of a logger implementation...
    public static class Logger
    {
        private static TextBox _GlobalAppLogTextBox;
        private static ListBox _CodeCompletionListView;
        private static ObservableCollection<CodeCompletionResult> _CodeCompletionOptions = null;

        public static void SetOutputViews(TextBox globalAppLogTextBox, ListBox codeCompletionListView)
        {
            _GlobalAppLogTextBox = globalAppLogTextBox;
            _CodeCompletionOptions = new ObservableCollection<CodeCompletionResult>();
            _CodeCompletionListView = codeCompletionListView;
            _CodeCompletionListView.Items.Clear();
            _CodeCompletionListView.ItemsSource = _CodeCompletionOptions;
        }

        public static void AppendLine(string value)
        {
            if (_GlobalAppLogTextBox == null) return;
            
            _GlobalAppLogTextBox.AppendText(value);
            if (!value.EndsWith("\r\n"))
                _GlobalAppLogTextBox.AppendText("\r\n");

            _GlobalAppLogTextBox.ScrollToEnd();
        }

        public static void AppendLine(string format, params object[] args)
        {
            if (_GlobalAppLogTextBox == null) return;

            var str = string.Format(format, args);
            _GlobalAppLogTextBox.AppendText(str);
            if (!str.EndsWith("\r\n"))
                _GlobalAppLogTextBox.AppendText("\r\n");

            _GlobalAppLogTextBox.ScrollToEnd();
        }

        public static void SetCodeCompletionOptions(IEnumerable<CodeCompletionResult> codeCompletionResults, CodeCompletionResult itemToSelect) // string completionWord = null)
        {
            var orderedResults = codeCompletionResults.OrderBy(x => x.CompletionText).ToArray();

            _CodeCompletionOptions.Clear();
            foreach (var x in orderedResults)
                _CodeCompletionOptions.Add(x);
            
            if (itemToSelect != null)
            {
                _CodeCompletionListView.ScrollIntoView(itemToSelect);
                _CodeCompletionListView.SelectedItem = itemToSelect;
            }
        }
    }
}
