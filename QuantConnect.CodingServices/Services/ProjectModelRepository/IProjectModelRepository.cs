using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuantConnect.CodingServices.Models;

namespace QuantConnect.CodingServices.Services.ProjectModelRepository
{
    public interface IProjectModelRepository
    {
        ProjectModel GetProject(int userId, int projectId);
        void SaveFileContent(int userId, int projectId, int fileId, string fileContent);
    }
}
