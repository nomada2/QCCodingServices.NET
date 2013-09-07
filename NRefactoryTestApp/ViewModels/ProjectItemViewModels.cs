using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MVVM;

namespace NRefactoryTestApp.ViewModels
{
    public enum ProjectItemType
    {
        Directory,
        CSharpSourceFile,
    }

    public interface IProjectItemViewModel : INotifyPropertyChanged
    {
        int Id { get; set; }
        int ProjectId { get; set; }
        //ProjectItemType Type { get; set; }
        string Name { get; set; }
        DateTime LastModified { get; set; }

        //BitmapSource MyIcon { get; set; }
        //string FullPathName { get; set; }

        ObservableCollection<IProjectItemViewModel> Children { get; }

        bool IsExpanded { get; set; }
        bool IncludeFileChildren { get; set; }
        //void DeleteChildren();
    }

    public abstract class ProjectItem : ViewModelBase, IProjectItemViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        //public ProjectItemType Type { get; set; }
        public DateTime LastModified { get; set; }

        // for display in tree
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value, "Name"); }
        }

        //protected BitmapSource myIcon;
        //public BitmapSource MyIcon
        //{
        //    get { return myIcon ?? (myIcon = GetMyIcon()); }
        //    set { myIcon = value; }
        //}
        //public abstract BitmapSource GetMyIcon();

        //public string FullPathName { get; set; }

        protected ObservableCollection<IProjectItemViewModel> children;
        public ObservableCollection<IProjectItemViewModel> Children
        {
            get { return children ?? (children = GetMyChildren()); }
            set { SetProperty(ref children, value, "Children"); }
        }
        public abstract ObservableCollection<IProjectItemViewModel> GetMyChildren();

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value, "IsExpanded"); }
        }

        public bool IncludeFileChildren { get; set; }

#if false
        // DeleteChildren, used to 
        // 1) remove old tree 2) set children=null, so a new tree is build

        // Question, not enough C#/Wpf knowledge:
        // - If we delete an ProjectItem in the root are all its children and corresponding treeview elements garbage collected??
        // - If not, does DeleteChildren() does the work?? 
        // - For now we decide to use DeleteChildren() but no destructor ~ProjectItem() that calls DeleteChildren.      
        public void DeleteChildren()
        {
            if (children != null)
            {
                // Console.WriteLine(this.FullPathName);

                for (int i = children.Count - 1; i >= 0; i--)
                {
                    children[i].DeleteChildren();
                    children[i] = null;
                    children.RemoveAt(i);
                }

                children = null;
            }
        }
#endif
    }

    public class ProjectItemContainerViewModel : ProjectItem
    {
        public ProjectItemContainerViewModel()
        {
            InitialChildren = new List<IProjectItemViewModel>();
        }

        public IList<IProjectItemViewModel> InitialChildren = null;

        public override ObservableCollection<IProjectItemViewModel> GetMyChildren()
        {
            ObservableCollection<IProjectItemViewModel> childrenList = new ObservableCollection<IProjectItemViewModel>(InitialChildren);
            return childrenList;
        }

        //public override BitmapSource GetMyIcon()
        //{
        //    // Note: introduce more "speaking" icons for RootItems
        //    string Param = "pack://application:,,,/" + "MyImages/bullet_blue.png";
        //    Uri uri1 = new Uri(Param, UriKind.RelativeOrAbsolute);
        //    return myIcon = BitmapFrame.Create(uri1);
        //}

    }

    public class ProjectDirectoryViewModel : ProjectItemContainerViewModel
    {
    }

    public class ProjectViewModel : ProjectItemContainerViewModel
    {
#if true
        // For now SelectedPath common to all trees
        RelayCommand selectedProjectItemFromTreeCommand;
        public ICommand SelectedProjectItemFromTreeCommand
        {
            get
            {
                return selectedProjectItemFromTreeCommand ??
                       (selectedProjectItemFromTreeCommand =
                              new RelayCommand(x => SelectedProjectItem = (x as IProjectItemViewModel)));
            }
        }

        private IProjectItemViewModel selectedProjectItem;
        public IProjectItemViewModel SelectedProjectItem
        {
            get { return selectedProjectItem; }
            set { SetProperty(ref selectedProjectItem, value, "SelectedProjectItem"); }
        }

#else
        // For now SelectedPath common to all trees
        RelayCommand selectedPathFromTreeCommand;
        public ICommand SelectedPathFromTreeCommand
        {
            get
            {
                return selectedPathFromTreeCommand ??
                       (selectedPathFromTreeCommand =
                              new RelayCommand(x => SelectedPath = (x as string)));
            }
        }

        // Selected path set by command call when item is clicked
        private string selectedPath;
        public string SelectedPath
        {
            get { return selectedPath; }
            set { SetProperty(ref selectedPath, value, "SelectedPath"); }
        }

#endif

        //private string selectedContent;
        //public string SelectedContent
        //{
        //    get { return selectedContent; }
        //    set { SetProperty(ref selectedContent, value, "SelectedContent"); }
        //}
    }

    public class ProjectFileViewModel : ProjectItem
    {
        private string content;
        public string Content
        {
            get { return content; }
            set { SetProperty(ref content, value, "Content"); }
        }

        private int caretIndex;
        public int CaretIndex
        {
            get { return caretIndex; }
            set { SetProperty(ref caretIndex, value, "CaretIndex"); }
        }

        private int caretLine;
        public int CaretLine
        {
            get { return caretLine; }
            set { SetProperty(ref caretLine, value, "CaretLine"); }
        }

        private int caretColumn;
        public int CaretColumn
        {
            get { return caretColumn; }
            set { SetProperty(ref caretColumn, value, "CaretColumn"); }
        }

        private string caretLocation;
        public string CaretLocation
        {
            get { return caretLocation; }
            set { SetProperty(ref caretLocation, value, "CaretLocation"); }
        }

        
        public override ObservableCollection<IProjectItemViewModel> GetMyChildren()
        {
            return null;
        }
    }
}
