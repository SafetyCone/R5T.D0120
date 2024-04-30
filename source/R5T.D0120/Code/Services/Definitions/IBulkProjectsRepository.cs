using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using R5T.T0064;
using R5T.T0238;
using R5T.T0247;


namespace R5T.D0120
{
    [RepositoryDefinitionMarker, ServiceDefinitionMarker]
    public interface IBulkProjectsRepository
    {
        public Task Clear();

        public Task<ProjectDescriptor[]> Get_All();
        public Task Set_All(IList<ProjectDescriptor> projects);
    }
}
