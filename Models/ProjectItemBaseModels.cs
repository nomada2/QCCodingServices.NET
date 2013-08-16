using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantConnect.Server.Autocomplete.Models
{
    public interface IProjectItemModel
    {
        int ProjectId { get; set; }
        int Id { get; set; }
        string Name { get; set; }
        DateTime LastModified { get; set; }
    }

    /// <summary>
    /// Abstract model representing a project item
    /// </summary>
    public abstract class ProjectItemModelBase : IProjectItemModel
    {
        /// <summary>
        /// The unique identifier for the project to which this project item belongs
        /// </summary>
        public virtual int ProjectId { get; set; }

        /// <summary>
        /// The unique identifier for the item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the item as given by the user
        /// </summary>
        public string Name { get; set; }


        // This field may be used to effectively represent the "version" of the entity.  Why do we need this?
        // If multiple requests (of the same type) are sent to the server for the same resource within 
        // a relatively short window of time, it's possible that by the time the server services those
        // requests, the client may receive them at around the same time or even in "reversed" order (since 
        // the server probably can't guarantee the order in which it services requests).  
        // In such an event, this field can be used by the client to disambiguate the responses from the server.
        // The server will return this value unmodified for the client's use.
        // ... but then again, this is me hypothesizing about what *could* happen, and I'm likely totally wrong.
        public DateTime LastModified { get; set; }
    }


    /// <summary>
    /// Abstract model representing a container of project items
    /// </summary>
    public abstract class ProjectItemContainerModel : ProjectItemModelBase
    {
        public IList<IProjectItemModel> Children { get; set; }

        public ProjectItemContainerModel()
        {
            Children = new List<IProjectItemModel>();
        }

        public ProjectFileModel FindFile(int fileId)
        {
            return ProjectModelUtils.FindFileInProjectItemCollection(Children, fileId);
        }

        public IEnumerable<ProjectFileModel> GetFileDescendants()
        {
            return ProjectModelUtils.GetFileDescendants(Children);
        }
    }
}
