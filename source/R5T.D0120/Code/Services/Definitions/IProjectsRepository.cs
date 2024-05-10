using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using R5T.L0089.T000;
using R5T.T0064;
using R5T.T0238;
using R5T.T0247;


namespace R5T.D0120
{
    /// <summary>
    /// 
    /// </summary>
    /// <!--
    /// Prior(s):
    /// R5T.S0112.Services.IProjectsRepository
    /// -->
	[RepositoryDefinitionMarker, ServiceDefinitionMarker]
    public interface IProjectsRepository : IServiceDefinition
    {
        public Task<Guid> Add(ProjectDescriptor project);

        public Task Remove(Guid identity);
        public Task Remove(string projectFilePath);

        public Task Update(ProjectDescriptor project);

        public Task<ProjectDescriptor> Get(Guid identity);
        public Task<ProjectDescriptor> Get(string projectFilePath);

        public Task<Dictionary<string, WasFound<ProjectDescriptor>>> Get(IEnumerable<string> projectFilePaths);

        public Task<bool> Exists(Guid identity);
        public Task<bool> Exists(string projectFilePath);
    }
}