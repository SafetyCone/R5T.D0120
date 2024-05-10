using System;
using System.Linq;

using R5T.H0001.Entities;
using R5T.T0132;
using R5T.T0238;


namespace R5T.D0120.I000
{
    [FunctionalityMarker]
    public partial interface IProjectDescriptorOperator : IFunctionalityMarker,
        T0238.IProjectDescriptorOperator
    {
        public ProjectDescriptor Get_ProjectDescriptor(Project projectEntity)
        {
            var projectDesriptor = new ProjectDescriptor(projectEntity.Identity)
            {
                FilePath = projectEntity.ProjectFilePath.Value,
                Name = projectEntity.Name,
                Description = projectEntity.Description,
                Type = projectEntity.Type,
                GitHubRepositoryUrl = projectEntity.GitHubRepositoryUrl,
                IsPrivate = projectEntity.IsPrivate,
                ProjectReferences = projectEntity.ProjectReferencesList.Mappings
                    .Select(x => x.ProjectFilePath.Value)
                    .ToArray()
            };

            return projectDesriptor;
        }
    }
}
