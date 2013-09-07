#if PRODUCTION_BUILD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuantConnect.Database;
using QuantConnect.Server.Autocomplete.Models;

namespace QuantConnect.Server.Autocomplete.Services.ProjectModelRepository
{
    public class PersistedProjectModelRepo : IProjectModelRepository
    {
        public ProjectModel GetProject(int userId, int projectId)
        {
            //Get files from database matching project ID:
            List<SimulatorFile> lSources = DB.GetProjectSource(userId, projectId, true);

            // Fabricate a ProjectModel from the list of files
            ProjectModel projectModel = new ProjectModel()
            {
                OwnerUserId = userId,
                ProjectId = projectId
            };
            foreach (SimulatorFile cFile in lSources)
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
#endif