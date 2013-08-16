using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantConnect.Server.Autocomplete.Models
{
    /// <summary>
    /// Model representing a project
    /// </summary>
    public class ProjectDto : ProjectItemContainerModel 
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
    }

    /// <summary>
    /// Model representing a directory
    /// </summary>
    public class ProjectDirectoryDto : ProjectItemContainerModel 
    {
    }

    /// <summary>
    /// Model representing a file
    /// </summary>
    public class ProjectFileDto : ProjectItemModelBase
    {
        public string Content { get; set; }
    }

}
