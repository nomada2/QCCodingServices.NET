using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace MVVM
{
    // Note on use of INotify: 

    // Use 1) INotify or 2) Dependancy property  3) None, can result in memory leaks
    // INotify vs Dep Prop: -- slower ++ Class not required to be depencancy object, more Poco => testable, serialisation

    // Both use repetative patterns and a magic string
    // INotify alternatives: use reflection, use lambda expression to test on magic string
    // See for one example  http://www.daedtech.com/wpf-and-notifying-property-change
    // See for less typing: http://www.codeproject.com/Articles/292073/Extending-Visual-Studio-Part-1-Creating-Code-Snipp

    // Basic wiring InotifyPropertyChanged
    // - PropertyChangedEventHandler is delegate
    // - Event PropertyChanged is called in class
    // - Outside class listeners to this event add delegate to this event

    // Basic wiring DependancyProperty , can also be put in base class. ********
    // Note that MainViewModel must derive from System.Windows.DependencyObject 
    // public static readonly System.Windows.DependencyProperty MyProp1Property =
    // DependencyProperty.Register("MyProp1Property", typeof(bool), typeof(MainViewModel));

    // public bool MyProp
    // {
    //    get { return (bool)GetValue(MainViewModel.MyProp1Property); }
    //    set { SetValue(MainViewModel.MyProp1Property, value); }
    // }

    public class ViewModelBase : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            checkIfPropertyNameExists(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Minor check, however in cuttting and pasting a new property we can make errors that pass this test
        // Oops, forget where I found these.. Suppose Sacha Barber introduced this test or at least blogged about it
        [Conditional("DEBUG")]
        private void checkIfPropertyNameExists(String propertyName)
        {
            Type type = this.GetType();
            Debug.Assert(
              type.GetProperty(propertyName) != null,
              propertyName + "property does not exist on object of type : " + type.FullName);
        }

        // From codeproject article Performance-and-Ideas-from-Anders-Hejlsberg-INotify (long link; google)
        // Note that we can use a text utility that checks/corrects field/propName on string basis
        public bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;

                checkIfPropertyNameExists(propertyName); 

                var handler = this.PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
                return true;
            }
            else 
            { 
                return false; 
            }
        }
    }
}
