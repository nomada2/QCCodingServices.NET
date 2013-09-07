using System.Collections.Generic;
using System.Collections.ObjectModel;
using QuantConnect.CodingServices.Models;

//using NRefactoryTestApp.models;

namespace NRefactoryTestApp.ViewModels
{
    public static class ProjectMVVMConverters
    {
        private static void CopyProjectContainerMembersFromModelToViewModel(ProjectItemContainerViewModel viewModel, ProjectItemContainerModel model)
        {
            viewModel.Id = model.Id;
            viewModel.ProjectId = model.ProjectId;
            viewModel.Name = model.Name;
            viewModel.LastModified = model.LastModified;
            //viewModel.Children = new ObservableCollection<IProjectItemViewModel>();
            foreach (var childModel in model.Children)
            {
                IProjectItemViewModel childViewModel = null;
                if (childModel is ProjectDirectoryDto)
                {
                    childViewModel = FromModelToViewModel((ProjectDirectoryDto)childModel);
                }
                else if (childModel is ProjectFileDto)
                {
                    childViewModel = FromModelToViewModel((ProjectFileDto)childModel);
                }
                viewModel.InitialChildren.Add(childViewModel);
            }
        }

        public static ProjectFileViewModel FromModelToViewModel(ProjectFileDto model)
        {
            var vm = new ProjectFileViewModel();
            vm.Id = model.Id;
            vm.ProjectId = model.ProjectId;
            vm.Name = model.Name;
            vm.LastModified = model.LastModified;
            vm.Content = model.Content;
            return vm;
        }

        public static ProjectViewModel FromModelToViewModel(ProjectDto model)
        {
            var vm = new ProjectViewModel();
            CopyProjectContainerMembersFromModelToViewModel(vm, model);
            return vm;
        }

        public static ProjectDirectoryViewModel FromModelToViewModel(ProjectDirectoryDto model)
        {
            var vm = new ProjectDirectoryViewModel();
            CopyProjectContainerMembersFromModelToViewModel(vm, model);
            return vm;
        }


        private static void CopyProjectContainerMembersFromViewModelToModel(ProjectItemContainerModel model, ProjectItemContainerViewModel viewModel)
        {
            model.Id = viewModel.Id;
            model.ProjectId = viewModel.ProjectId;
            model.Name = viewModel.Name;
            model.LastModified = viewModel.LastModified;
            //model.Children = new List<IProjectItemModel>();
            foreach (var childViewModel in viewModel.Children)
            {
                IProjectItemModel childModel = null;
                if (childViewModel is ProjectDirectoryViewModel)
                {
                    childModel = FromViewModelToModel((ProjectDirectoryViewModel)childViewModel);
                }
                else if (childViewModel is ProjectFileViewModel)
                {
                    childModel = FromViewModelToModel((ProjectFileViewModel)childViewModel);
                }
                model.Children.Add(childModel);
            }
        }

        public static ProjectFileDto FromViewModelToModel(ProjectFileViewModel viewModel)
        {
            var model = new ProjectFileDto();
            model.Id = viewModel.Id;
            model.ProjectId = viewModel.ProjectId;
            model.Name = viewModel.Name;
            model.LastModified = viewModel.LastModified;
            model.Content = viewModel.Content;
            return model;
        }

        public static ProjectDto FromViewModelToModel(ProjectViewModel viewModel)
        {
            var model = new ProjectDto();
            CopyProjectContainerMembersFromViewModelToModel(model, viewModel);
            return model;
        }

        public static ProjectDirectoryDto FromViewModelToModel(ProjectDirectoryViewModel viewModel)
        {
            var model = new ProjectDirectoryDto();
            CopyProjectContainerMembersFromViewModelToModel(model, viewModel);
            return model;
        }
    }
}