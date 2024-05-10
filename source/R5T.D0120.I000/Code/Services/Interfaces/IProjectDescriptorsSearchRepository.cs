using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

using R5T.H0001;
using R5T.T0064;
using R5T.T0238;
using R5T.T0247;


namespace R5T.D0120.I000
{
    /// <summary>
    /// 
    /// </summary>
	[RepositoryDefinitionMarker, ServiceDefinitionMarker]
    public interface IProjectDescriptorsSearchRepository : IServiceDefinition
    {
        public Task<ProjectDescriptor[]> Find_By(Expression<Func<IProjectDescriptor, bool>> searchExpression);
    }
}