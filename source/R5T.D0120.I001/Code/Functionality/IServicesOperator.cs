using System;

using Microsoft.Extensions.DependencyInjection;

using R5T.T0132;
using R5T.T0248;


namespace R5T.D0120.I001
{
    [FunctionalityMarker]
    public partial interface IServicesOperator : IFunctionalityMarker
    {
        public void Add_InMemoryProjectsRepository(
            IServiceCollection services,
            out ServiceToken<InMemoryProjectsRepository> inMemoryProjectsRepositoryServiceToken,
            out ServiceToken<IProjectsRepository> projectsRepositoryServiceToken,
            out ServiceToken<IBulkProjectsRepository> bulkProjectsRepositoryServiceToken)
        {
            services.AddSingleton<InMemoryProjectsRepository>();
            services.AddSingleton<IProjectsRepository>(serviceProvider => serviceProvider.GetService<InMemoryProjectsRepository>());
            services.AddSingleton<IBulkProjectsRepository>(serviceProvider => serviceProvider.GetService<InMemoryProjectsRepository>());
        }
    }
}
