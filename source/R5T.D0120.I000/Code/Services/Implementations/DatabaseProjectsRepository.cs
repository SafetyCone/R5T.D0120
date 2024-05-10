using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using R5T.H0001;
using R5T.H0001.Entities;
using R5T.T0064;
using R5T.T0238;
using R5T.T0247;


namespace R5T.D0120.I000
{
    [RepositoryImplementationMarker, ServiceImplementationMarker]
    public class DatabaseProjectsRepository : IRepositoryImplementationMarker, IServiceImplementation,
        IProjectsRepository,
        IBulkProjectsRepository,
        IProjectDescriptorsSearchRepository
    {
        private ProjectsDbContext DbContext { get; }


        public DatabaseProjectsRepository(
            ProjectsDbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        public async Task<Guid> Add(ProjectDescriptor projectDescriptor)
        {
            Instances.ProjectDescriptorOperator.Verify_IdentityNotSet(projectDescriptor);

            var projectFilePathsRequired = projectDescriptor.ProjectReferences
                .Append(projectDescriptor.FilePath)
                .Now();

            var projectFilePathEntities = await this.DbContext.ProjectFilePaths
                .Where(x => projectFilePathsRequired.Contains(x.Value))
                .ToDictionaryAsync(
                    x => x.Value);

            var projectFilePathsToAdd = projectFilePathsRequired
                .Except(projectFilePathEntities.Keys)
                .Now();

            var projectFilePathEntitiesByProjectFilePath = projectFilePathsRequired
                .Select(projectFilePath =>
                {
                    if (!projectFilePathEntities.TryGetValue(projectFilePath, out var projectFilePathEntity))
                    {
                        projectFilePathEntity = new ProjectFilePath
                        {
                            Value = projectFilePath
                        };

                        this.DbContext.ProjectFilePaths.Add(projectFilePathEntity);
                    }

                    return projectFilePathEntity;
                })
                .ToDictionary(
                    x => x.Value);

            var projectReferencesListEntity = new ProjectFilePathList();

            this.DbContext.ProjectFilePathLists.Add(projectReferencesListEntity);

            foreach (var projectReferenceProjectFilePath in projectDescriptor.ProjectReferences)
            {
                var projectFilePathEntity = projectFilePathEntitiesByProjectFilePath[projectReferenceProjectFilePath];

                var mappingEntity = new ProjectFilePathListMapping
                {
                    ProjectFilePath = projectFilePathEntity,
                    ProjectFilePathList = projectReferencesListEntity
                };

                this.DbContext.ProjectFilePathListMappings.Add(
                    mappingEntity);
            }

            var projectEntity = new Project
            {
                Identity = Instances.GuidOperator.New(),

                Name = projectDescriptor.Name,
                Description = projectDescriptor.Description,
                Type = projectDescriptor.Type,

                GitHubRepositoryUrl = projectDescriptor.GitHubRepositoryUrl,
                IsPrivate = projectDescriptor.IsPrivate,

                ProjectFilePath = projectFilePathEntitiesByProjectFilePath[projectDescriptor.FilePath],
                ProjectReferencesList = projectReferencesListEntity,
            };

            this.DbContext.Projects.Add(projectEntity);

            await this.DbContext.SaveChangesAsync();

            return projectEntity.Identity;
        }

        public async Task Remove(Guid identity)
        {
            // Do not delete the project file paths (since they might be in use by other projects), so no need to include them.
            var projectEntity = await this.DbContext.Projects
                .Where(x => x.Identity == identity)
                .Include(x => x.ProjectReferencesList).ThenInclude(x => x.Mappings)
                .FirstAsync();

            this.DbContext.ProjectFilePathListMappings.RemoveRange(projectEntity.ProjectReferencesList.Mappings);
            this.DbContext.ProjectFilePathLists.Remove(projectEntity.ProjectReferencesList);
            this.DbContext.Remove(projectEntity);

            await this.DbContext.SaveChangesAsync();
        }

        public async Task Remove(string projectFilePath)
        {
            // Do not delete the project file paths (since they might be in use by other projects), so no need to include them.
            var projectEntity = await this.DbContext.Projects
                .Where(x => x.ProjectFilePath.Value == projectFilePath)
                .Include(x => x.ProjectReferencesList).ThenInclude(x => x.Mappings)
                .FirstAsync();

            this.DbContext.ProjectFilePathListMappings.RemoveRange(projectEntity.ProjectReferencesList.Mappings);
            this.DbContext.ProjectFilePathLists.Remove(projectEntity.ProjectReferencesList);
            this.DbContext.Remove(projectEntity);

            await this.DbContext.SaveChangesAsync();
        }

        public async Task Update(ProjectDescriptor project)
        {
            var projectEntity = await this.DbContext.Projects
                .Where(x => x.Identity == project.Identity)
                .Include(x => x.ProjectFilePath)
                .Include(x => x.ProjectReferencesList).ThenInclude(x => x.Mappings).ThenInclude(x => x.ProjectFilePath)
                .FirstAsync();

            projectEntity.Name = project.Name;
            projectEntity.Description = project.Description;
            projectEntity.Type = project.Type;

            projectEntity.GitHubRepositoryUrl = project.GitHubRepositoryUrl;
            projectEntity.IsPrivate = project.IsPrivate;

            if (projectEntity.ProjectFilePath.Value != project.FilePath)
            {
                projectEntity.ProjectFilePath = new ProjectFilePath
                {
                    Value = project.FilePath
                };

                // Don't worry about deleting the old path.
            }

            var projectFilePathEntities = await this.DbContext.ProjectFilePaths
                .Where(x => project.ProjectReferences.Contains(x.Value))
                .ToDictionaryAsync(
                    x => x.Value);

            var projectFilePathsToAdd = project.ProjectReferences
                .Except(projectFilePathEntities.Keys)
                .Now();

            var projectFilePathEntitiesByProjectFilePath = project.ProjectReferences
                .Select(projectFilePath =>
                {
                    if (!projectFilePathEntities.TryGetValue(projectFilePath, out var projectFilePathEntity))
                    {
                        projectFilePathEntity = new ProjectFilePath
                        {
                            Value = projectFilePath
                        };

                        this.DbContext.ProjectFilePaths.Add(projectFilePathEntity);
                    }

                    return projectFilePathEntity;
                })
                .ToDictionary(
                    x => x.Value);

            foreach (var projectFilePath in projectFilePathsToAdd)
            {
                var projectFilePathEntity = projectFilePathEntitiesByProjectFilePath[projectFilePath];

                var mappingEntity = new ProjectFilePathListMapping
                {
                    ProjectFilePath = projectFilePathEntity,
                    ProjectFilePathList = projectEntity.ProjectReferencesList
                };

                this.DbContext.ProjectFilePathListMappings.Add(
                    mappingEntity);
            }

            var projectReferenceMappingsByProjectFilePath = projectEntity.ProjectReferencesList.Mappings
                .ToDictionary(
                    x => x.ProjectFilePath.Value);

            var projectFilePathsToRemove = projectReferenceMappingsByProjectFilePath.Keys
                .Except(project.ProjectReferences)
                .Now();

            foreach (var projectFilePath in projectFilePathsToRemove)
            {
                var mappingEntity = projectReferenceMappingsByProjectFilePath[projectFilePath];

                this.DbContext.ProjectFilePathListMappings.Remove(mappingEntity);
            }

            await this.DbContext.SaveChangesAsync();
        }

        public async Task<ProjectDescriptor> Get(Guid identity)
        {
            var projectEntity = await this.DbContext.Projects
                .Where(x => x.Identity == identity)
                .Include(x => x.ProjectFilePath)
                .Include(x => x.ProjectReferencesList).ThenInclude(x => x.Mappings).ThenInclude(x => x.ProjectFilePath)
                .FirstAsync();

            var output = Instances.ProjectDescriptorOperator.Get_ProjectDescriptor(projectEntity);
            return output;
        }

        public async Task<ProjectDescriptor> Get(string projectFilePath)
        {
            var projectEntity = await this.DbContext.Projects
                .Where(x => x.ProjectFilePath.Value == projectFilePath)
                .Include(x => x.ProjectFilePath)
                .Include(x => x.ProjectReferencesList).ThenInclude(x => x.Mappings).ThenInclude(x => x.ProjectFilePath)
                .FirstAsync();

            var output = Instances.ProjectDescriptorOperator.Get_ProjectDescriptor(projectEntity);
            return output;
        }

        public async Task<bool> Exists(Guid identity)
        {
            var output = await this.DbContext.Projects
                .Where(x => x.Identity == identity)
                .AnyAsync();

            return output;
        }

        public async Task<bool> Exists(string projectFilePath)
        {
            var output = await this.DbContext.Projects
                .Where(x => x.ProjectFilePath.Value == projectFilePath)
                .AnyAsync();

            return output;
        }

        public async Task Clear()
        {
            // Dependency order matters here.
            await this.DbContext.Projects.ExecuteDeleteAsync();
            await this.DbContext.ProjectFilePathListMappings.ExecuteDeleteAsync();
            await this.DbContext.ProjectFilePathLists.ExecuteDeleteAsync();
            await this.DbContext.ProjectFilePaths.ExecuteDeleteAsync();
        }

        public async Task<ProjectDescriptor[]> Get_All()
        {
            var projectEntities = await this.DbContext.Projects
                .Include(x => x.ProjectFilePath)
                .Include(x => x.ProjectReferencesList).ThenInclude(x => x.Mappings).ThenInclude(x => x.ProjectFilePath)
                .ToListAsync();

            var projects = projectEntities
                .Select(Instances.ProjectDescriptorOperator.Get_ProjectDescriptor)
                .ToArray();

            return projects;
        }

        public async Task Set_All(IList<ProjectDescriptor> projects)
        {
            // Verify inputs.
            foreach (var project in projects)
            {
                Instances.ProjectDescriptorOperator.Verify_IdentityNotSet(project);
            }

            await this.Clear();

            // Now bulk add all projects.

            // Assume Clear() has been called, this there are no project file paths.
            var allProjectFilePaths = projects
                .Select(x => x.FilePath)
                .Append(projects
                    .SelectMany(x => x.ProjectReferences))
                .Distinct()
                .ToArray();

            // Create all project file entities.
            var projectFilePathEntitiesByProjectFilePath = new Dictionary<string, ProjectFilePath>();

            foreach (var projectFilePath in allProjectFilePaths)
            {
                var projectFilePathEntity = new ProjectFilePath
                {
                    Value = projectFilePath,
                };

                projectFilePathEntitiesByProjectFilePath.Add(projectFilePathEntity.Value, projectFilePathEntity);
            }

            // Add all project file path entities.
            this.DbContext.ProjectFilePaths.AddRange(
                projectFilePathEntitiesByProjectFilePath.Values);

            // Create all project reference list entities.
            var projectReferenceListEntitiesByProjectFilePath = new Dictionary<string, ProjectFilePathList>();

            foreach (var projectFilePath in allProjectFilePaths)
            {
                var projectReferencesListEntity = new ProjectFilePathList();

                projectReferenceListEntitiesByProjectFilePath.Add(projectFilePath, projectReferencesListEntity);
            }

            // Add all project reference list entities.
            this.DbContext.ProjectFilePathLists.AddRange(
                projectReferenceListEntitiesByProjectFilePath.Values);

            // Now add all project reference list-to-project file mappings.
            foreach (var project in projects)
            {
                var projectReferenceListEntity = projectReferenceListEntitiesByProjectFilePath[project.FilePath];

                foreach (var projectFilePath in project.ProjectReferences)
                {
                    var projectFilePathEntity = projectFilePathEntitiesByProjectFilePath[projectFilePath];

                    var mappingEntity = new ProjectFilePathListMapping
                    {
                        ProjectFilePath = projectFilePathEntity,
                        ProjectFilePathList = projectReferenceListEntity,
                    };

                    this.DbContext.ProjectFilePathListMappings.Add(mappingEntity);
                }
            }

            // Create and add all project entities.
            foreach (var project in projects)
            {
                var projectEntity = new Project
                {
                    // Add a new key value.
                    Identity = Instances.GuidOperator.New(),

                    Name = project.Name,
                    Description = project.Description,
                    Type = project.Type,

                    GitHubRepositoryUrl = project.GitHubRepositoryUrl,
                    IsPrivate = project.IsPrivate,

                    ProjectFilePath = projectFilePathEntitiesByProjectFilePath[project.FilePath],
                    ProjectReferencesList = projectReferenceListEntitiesByProjectFilePath[project.FilePath],
                };

                this.DbContext.Projects.Add(projectEntity);
            }

            // Oh boy, big save!
            await this.DbContext.SaveChangesAsync();
        }

        public async Task<ProjectDescriptor[]> Find_By(Expression<Func<IProjectDescriptor, bool>> searchExpression)
        {
            var projectEntities = await this.DbContext.Projects
                .Where(searchExpression)
                .Cast<Project>()
                .Include(x => x.ProjectFilePath)
                .Include(x => x.ProjectReferencesList).ThenInclude(x => x.Mappings).ThenInclude(x => x.ProjectFilePath)
                .ToListAsync();

            var output = projectEntities
                .Select(Instances.ProjectDescriptorOperator.Get_ProjectDescriptor)
                .ToArray();

            return output;
        }

        public async Task<Dictionary<string, L0089.T000.WasFound<ProjectDescriptor>>> Get(IEnumerable<string> projectFilePaths)
        {
            var projectFilePaths_Hash = Instances.HashSetOperator.From(projectFilePaths);

            var projectEntities = await this.DbContext.Projects
                // Use the hash, since it will already be de-duplicated.
                .Where(x => projectFilePaths_Hash.Contains(x.ProjectFilePath.Value))
                .Include(x => x.ProjectFilePath)
                .Include(x => x.ProjectReferencesList).ThenInclude(x => x.Mappings).ThenInclude(x => x.ProjectFilePath)
                .ToListAsync();

            var projectDescriptors = projectEntities
                .Select(Instances.ProjectDescriptorOperator.Get_ProjectDescriptor)
                .ToArray();

            var output = projectDescriptors.ToDictionary(
                x => x.FilePath,
                x => Instances.WasFoundOperator.Get_New_Found(x));

            var missingProjectFilePaths = projectFilePaths_Hash
                .Except(output.Keys)
                ;

            foreach (var missingProjectFilePath in missingProjectFilePaths)
            {
                output.Add(
                    missingProjectFilePath,
                    Instances.WasFoundOperator.Get_New_NotFound<ProjectDescriptor>());
            }

            return output;
        }
    }
}
