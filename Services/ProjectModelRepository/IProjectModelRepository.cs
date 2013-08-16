using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuantConnect.Server.Autocomplete.Models;

namespace QuantConnect.Server.Autocomplete.Services.ProjectModelRepository
{
    public interface IProjectModelRepository
    {
        ProjectModel GetProject(int userId, int projectId);
        void SaveFileContent(int userId, int projectId, int fileId, string fileContent);
    }
}
