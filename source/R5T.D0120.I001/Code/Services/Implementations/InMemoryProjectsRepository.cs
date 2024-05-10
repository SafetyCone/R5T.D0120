using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using R5T.L0066.Extensions;
using R5T.L0089.T000;
using R5T.T0064;
using R5T.T0238;
using R5T.T0247;


namespace R5T.D0120.I001
{
    [RepositoryImplementationMarker, ServiceImplementationMarker]
    public class InMemoryProjectsRepository : IServiceImplementation,
        IProjectsRepository,
        IBulkProjectsRepository
    {
        private HashSet<Guid> Identities { get; set; } = [];
        private HashSet<string> ProjectFilePaths { get; set; } = [];
        private List<ProjectDescriptor> ProjectDescriptors { get; set; } = [];


        private void Verify_DoesNotHaveProject(
            string projectFilePath)
        {
            var hasProject = this.ProjectFilePaths.Contains(projectFilePath);
            if(hasProject)
            {
                throw new Exception($"Project with file path already exists:\n\t{projectFilePath}");
            }
        }

        private void Verify_CanAdd(ProjectDescriptor projectDescriptor)
        {
            Instances.ProjectDescriptorOperator.Verify_IdentityNotSet(projectDescriptor);

            this.Verify_DoesNotHaveProject(projectDescriptor.FilePath);
        }

        private void Add_Internal(ProjectDescriptor project)
        {
            this.Identities.Add(project.Identity);
            this.ProjectFilePaths.Add(project.FilePath);
            this.ProjectDescriptors.Add(project);
        }


        public Task<Guid> Add(ProjectDescriptor project)
        {
            this.Verify_CanAdd(project);

            var instanceToAdd = Instances.ProjectDescriptorOperator.Clone_Set_Identity(project);

            this.Add_Internal(instanceToAdd);

            return Task.FromResult(instanceToAdd.Identity);
        }

        private void Clear_Internal()
        {
            this.Identities.Clear();
            this.ProjectFilePaths.Clear();
            this.ProjectDescriptors.Clear();
        }

        public Task Clear()
        {
            this.Clear_Internal();

            return Task.CompletedTask;
        }

        public Task<bool> Exists(Guid identity)
        {
            var output = this.Identities.Contains(identity);
            
            return Task.FromResult(output);
        }

        public Task<bool> Exists(string projectFilePath)
        {
            var output = this.ProjectFilePaths.Contains(projectFilePath);

            return Task.FromResult(output);
        }

        public Task<ProjectDescriptor> Get(Guid identity)
        {
            var output = this.ProjectDescriptors
                .Where(x => x.Identity == identity)
                .Single();

            return Task.FromResult(output);
        }

        public Task<ProjectDescriptor> Get(string projectFilePath)
        {
            var output = this.ProjectDescriptors
                .Where(x => x.FilePath == projectFilePath)
                .Single();

            return Task.FromResult(output);
        }

        public Task<ProjectDescriptor[]> Get_All()
        {
            var output = this.ProjectDescriptors.ToArray();

            return Task.FromResult(output);
        }

        private void Remove_Internal(ProjectDescriptor project)
        {
            this.Identities.Remove(project.Identity);
            this.ProjectFilePaths.Remove(project.FilePath);
            this.ProjectDescriptors.Remove(project);
        }

        public async Task Remove(Guid identity)
        {
            var instance = await this.Get(identity);

            this.Remove_Internal(instance);
        }

        public async Task Remove(string projectFilePath)
        {
            var instance = await this.Get(projectFilePath);

            this.Remove_Internal(instance);
        }

        public Task Set_All(IList<ProjectDescriptor> projects)
        {
            // Verify keys.
            var duplicateProjectFilePaths = projects
                .GroupBy(x => x.FilePath)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .Now();

            var anyDuplicateProjectFilePaths = duplicateProjectFilePaths.Any();
            if(anyDuplicateProjectFilePaths)
            {
                throw new Exception("There were some duplicate file paths.");
            }

            var withNonDefaultIdentities = projects
                .Where(x => Instances.DefaultOperator.Is_NotDefault(x.Identity))
                .Now();

            var anyWithNonDefaultIdentities = withNonDefaultIdentities.Any();

            // Ok, now add.
            this.Clear_Internal();

            var instancesToAdd = projects
                .Select(Instances.ProjectDescriptorOperator.Clone_Set_Identity)
                ;

            this.ProjectDescriptors.AddRange(instancesToAdd);

            this.ProjectFilePaths.Add_Range(this.ProjectDescriptors
                .Select(x => x.FilePath));

            this.Identities.Add_Range(this.ProjectDescriptors
                .Select(x => x.Identity));

            return Task.CompletedTask;
        }

        public Task Update(ProjectDescriptor project)
        {
            // Must have a non-default identity.
            var identityIsDefault = Instances.DefaultOperator.Is_Default(project.Identity);
            if(identityIsDefault)
            {
                throw new Exception("Cannot update project: missing identity");
            }

            var instanceToUpdate = this.ProjectDescriptors
                .Where(x => x.Identity == project.Identity)
                .Single();

            Instances.ProjectDescriptorOperator.Clone_Set_Data(
                project,
                instanceToUpdate);

            return Task.CompletedTask;
        }

        public Task<Dictionary<string, WasFound<ProjectDescriptor>>> Get(IEnumerable<string> projectFilePaths)
        {
            var projectDescriptorsByProjectFilePath_Temp = this.ProjectDescriptors
                .ToDictionary(
                    x => x.FilePath);

            var output = projectFilePaths
                .Select(projectFilePath =>
                {
                    var exists = projectDescriptorsByProjectFilePath_Temp.TryGetValue(
                        projectFilePath,
                        out var projectDescriptor);

                    var output = exists
                        ? Instances.WasFoundOperator.Get_New_Found(projectDescriptor)
                        : Instances.WasFoundOperator.Get_New_NotFound<ProjectDescriptor>()
                        ;

                    return (ProjectFilePath: projectFilePath, ProjectDescriptor: output);
                })
                .ToDictionary(
                    x => x.ProjectFilePath,
                    x => x.ProjectDescriptor);

            return Task.FromResult(output);
        }
    }
}
