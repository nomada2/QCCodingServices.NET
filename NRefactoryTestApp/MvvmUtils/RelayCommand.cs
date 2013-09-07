using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics;

// A note on ViewModel, Commands and RelayCommand

// ViewModel has properties. 
// 1) Single values (INotify.. or DependencyObject), 
// 2) Collections ObservableCollection<> 
// 3) Commands implementing ICommand (e.g. void Execute + bool CanExecute)

// The RelayCommand implements Execute + CanExecute and its constructor can bind _execute and _canExecute to specified user procedures
// This reduces somewhat the repetative code in de ViewModel Itself. 

// Example TestCommand:

// RelayCommand _testCommand;
// public ICommand TestCommand
// {
//    get
//    {
//       return _testCommand ?? ( _testCommand = new RelayCommand ( x => MessageBox.Show("MiniTest succeeded") ) );
//       // or:  (param => this.Command1Function(),  param => this.Command1CanExecuteTest() );
//    }
// }

// Note: not all UI elements have a command property for binding
// Hoewever by using de Blend3 Microsoft.Expression.Interactions Systems.Windows.Interactivity dll's 
// we can use attached behaviours to bind to every event one or more commands. 
// XAML Example:
        //<Rectangle Width="30" Height="30">
        //    <i:Interaction.Triggers>
        //        <i:EventTrigger EventName="MouseEnter">
        //            <local:CommandAction Command="{Binding HoverOnRecCommand}" SyncOwnerIsEnabled="True"/>
        //        </i:EventTrigger>
        //    </i:Interaction.Triggers>
        //</Rectangle>

namespace MVVM
{
    //  Source: Josh Smith in WPF Apps With The Model-View-ViewModel Design Pattern, 
    //  http://msdn.microsoft.com/en-us/magazine/dd419663.aspx

    public class RelayCommand : ICommand
    {
        #region Fields

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        #endregion // Fields

        #region Constructors

        // constructor no CanExecute
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion // ICommand Members
    }
}
