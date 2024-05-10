using System;

using Microsoft.Extensions.DependencyInjection;

using R5T.T0248;


namespace R5T.D0120.I001
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Add_InMemoryProjectsRepository(this IServiceCollection services,
            out ServiceToken<InMemoryProjectsRepository> inMemoryProjectsRepository,
            out ServiceToken<IProjectsRepository> projectsRepositoryServiceToken,
            out ServiceToken<IBulkProjectsRepository> bulkProjectsRepositoryServiceToken)
        {
            Instances.ServicesOperator.Add_InMemoryProjectsRepository(
                services,
                out inMemoryProjectsRepository,
                out projectsRepositoryServiceToken,
                out bulkProjectsRepositoryServiceToken);

            return services;
        }
    }
}
