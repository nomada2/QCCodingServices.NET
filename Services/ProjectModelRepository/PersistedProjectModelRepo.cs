/*
 * QUANTCONNECT.COM
 * QC.Autocomplete by Paul Miller
 * January 2013
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuantConnect.Server.Autocomplete.Models;


namespace QuantConnect.Server.Autocomplete.Services.ProjectModelRepository
{
    public class PersistedProjectModelRepo : IProjectModelRepository
    {
        public ProjectModel GetProject(int userId, int projectId)
        {

            // Fabricate a ProjectModel from the list of files
            ProjectModel projectModel = new ProjectModel()
            {
                OwnerUserId = userId,
                ProjectId = projectId
            };


            // *********************************************************
            //Get files from database, make into a project model for the autocomplete:
            List<File> lSources = [ FETCH REST CODE ];

            foreach (File cFile in lSources)
            {
                var fileModel = new ProjectFileModel()
                {
                    ProjectId = projectId,
                    Id = cFile.id,
                    Name = cFile.name,
                    Content = cFile.content
                };
                projectModel.Children.Add(fileModel);
            }

            return projectModel;
        }

        public void SaveFileContent(int userId, int projectId, int fileId, string fileContent)
        {
            throw new NotImplementedException();
        }
    }
}
