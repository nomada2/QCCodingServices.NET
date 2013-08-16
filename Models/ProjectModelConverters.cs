using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Completion;

namespace QuantConnect.Server.Autocomplete.Models
{
    public static class ProjectModelConverters
    {
        private static void CopyIProjectItemModelMembers(IProjectItemModel target, IProjectItemModel source)
        {
            target.ProjectId = source.ProjectId;
            target.Id = source.Id;
            target.Name = source.Name;
            target.LastModified = source.LastModified;
        }

        private static void CopyProjectContainerItemsFromModelToDto(ProjectItemContainerModel dto, ProjectItemContainerModel model)
        {
            //dto.Children = new List<IProjectItemModel>();
            foreach (var childModel in model.Children)
            {
                IProjectItemModel childDto = null;
                if (childModel is ProjectDirectoryModel)
                {
                    childDto = FromModelToDto((ProjectDirectoryModel)childModel);
                }
                else if (childModel is ProjectFileModel)
                {
                    childDto = FromModelToDto((ProjectFileModel)childModel);
                }
                dto.Children.Add(childDto);
            }
        }

        public static ProjectFileDto FromModelToDto(ProjectFileModel model)
        {
            var dto = new ProjectFileDto();
            CopyIProjectItemModelMembers(dto, model);
            dto.Content = model.Content;
            return dto;
        }

        public static ProjectDto FromModelToDto(ProjectModel model)
        {
            var dto = new ProjectDto();
            dto.OwnerUserId = model.OwnerUserId;
            CopyIProjectItemModelMembers(dto, model);
            CopyProjectContainerItemsFromModelToDto(dto, model);
            return dto;
        }

        public static ProjectDirectoryDto FromModelToDto(ProjectDirectoryModel model)
        {
            var dto = new ProjectDirectoryDto();
            CopyIProjectItemModelMembers(dto, model);
            CopyProjectContainerItemsFromModelToDto(dto, model);
            return dto;
        }


        private static void CopyProjectContainerItemsFromDtoToModel(ProjectItemContainerModel model, ProjectItemContainerModel dto)
        {
            //CopyIProjectItemModelMembers(model, dto);
            //model.Children = new List<IProjectItemModel>();
            foreach (var childDto in dto.Children)
            {
                IProjectItemModel childModel = null;
                if (childDto is ProjectDirectoryDto)
                {
                    childModel = FromDtoToModel((ProjectDirectoryDto)childDto);
                }
                else if (childDto is ProjectFileDto)
                {
                    childModel = FromDtoToModel((ProjectFileDto)childDto);
                }
                model.Children.Add(childModel);
            }
        }

        public static ProjectFileModel FromDtoToModel(ProjectFileDto dto)
        {
            var model = new ProjectFileModel();
            CopyIProjectItemModelMembers(model, dto);
            model.Content = dto.Content;
            return model;
        }

        public static ProjectModel FromDtoToModel(ProjectDto dto)
        {
            var model = new ProjectModel();
            model.OwnerUserId = dto.OwnerUserId;
            CopyIProjectItemModelMembers(model, dto);
            CopyProjectContainerItemsFromDtoToModel(model, dto);
            return model;
        }

        public static ProjectDirectoryModel FromDtoToModel(ProjectDirectoryDto dto)
        {
            var model = new ProjectDirectoryModel();
            CopyIProjectItemModelMembers(model, dto);
            CopyProjectContainerItemsFromDtoToModel(model, dto);
            return model;
        }
    }

}
