using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem;

namespace QuantConnect.Server.Autocomplete.Models
{
    /// <summary>
    /// Model representing a project
    /// </summary>
    public class ProjectModel : ProjectItemContainerModel 
    {
        /// <summary>
        /// A project's ID is its ProjectId
        /// </summary>
        public override int ProjectId
        {
            get { return Id; }
            set { Id = value; }
        }

        /// <summary>
        /// The user Id of the owner
        /// </summary>
        public int OwnerUserId { get; set; }


        #region NRefactory Objects
        public IProjectContent ProjectContent { get; set; }
        public CSharpCompletionEngine CompletionEngine { get; set; }
        #endregion
    }

    /// <summary>
    /// Model representing a directory
    /// </summary>
    public class ProjectDirectoryModel : ProjectItemContainerModel 
    {
    }

    /// <summary>
    /// Model representing a file
    /// </summary>
    public class ProjectFileModel : ProjectItemModelBase
    {
        public string Content { get; set; }

        #region NRefactory Objects
        public CSharpParser Parser { get; set; }
        public SyntaxTree SyntaxTree { get; set; }
        public CSharpUnresolvedFile UnresolvedFile { get; set; }
        #endregion
    }

    public static class ProjectModelUtils
    {
        public static IEnumerable<ProjectFileModel> GetFileDescendants(IList<IProjectItemModel> projectItems)
        {
            foreach (var item in projectItems)
            {
                if (item is ProjectFileModel)
                {
                    yield return (ProjectFileModel)item;
                }

                if (item is ProjectDirectoryModel)
                {
                    foreach (var dirItem in GetFileDescendants(((ProjectDirectoryModel)item).Children))
                        yield return dirItem;
                }
            }
        }

        /// <summary>
        /// Given an IList of IProjectItemModel, find the file embedded within the heirarchy having the given file ID.
        /// NOTE:  This method could probably be promoted to the status of a public helper method in some other static class.
        /// NOTE:  It would also be easy to turn this method into a general-purpose visitor...
        /// </summary>
        /// <param name="projectItems"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public static ProjectFileModel FindFileInProjectItemCollection(IList<IProjectItemModel> projectItems, int fileId)
        {
            foreach (var item in projectItems)
            {
                if (item is ProjectFileModel && ((ProjectFileModel)item).Id == fileId)
                {
                    // target has been acquired!
                    return (ProjectFileModel)item;
                }

                if (item is ProjectDirectoryModel)
                {
                    var possibleTarget = FindFileInProjectItemCollection(((ProjectDirectoryModel)item).Children, fileId);
                    // if we've found our target, return it!
                    if (possibleTarget != null)
                        return possibleTarget;
                }

            }
            // if we've gotten here, then that means we haven't found the target file
            return null;
        }

    }
}
