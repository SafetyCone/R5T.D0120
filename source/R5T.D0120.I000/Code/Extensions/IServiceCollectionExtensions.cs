using System;

using Microsoft.Extensions.DependencyInjection;

using R5T.H0001;
using R5T.T0248;


namespace R5T.D0120.I000
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection Add_ProjectsRepositoryDatabase_UsingLocalDevelopment(this IServiceCollection services,
            out ServiceToken<ProjectsDbContext> projectsDbContextToken,
            out ServiceToken<IProjectsRepository> projectsRepositoryToken,
            out ServiceToken<IBulkProjectsRepository> bulkProjectsRepositoryToken,
            out ServiceToken<IProjectDescriptorsSearchRepository> projectDescriptorsSearchRepositoryToken)
        {
            Instances.ServicesOperator.Add_DatabaseProjectsRepository_UsingLocalDevelopment(
                services,
                out projectsDbContextToken,
                out projectsRepositoryToken,
                out bulkProjectsRepositoryToken,
                out projectDescriptorsSearchRepositoryToken);

            return services;
        }

        public static IServiceCollection Add_ProjectsDbContext_UsingLocalDevelopment(this IServiceCollection services,
            out ServiceToken<ProjectsDbContext> projectsDbContextToken)
        {
            Instances.ServicesOperator.Add_ProjectsDbContext_UsingLocalDevelopment(
                services,
                out projectsDbContextToken);

            return services;
        }

        public static IServiceCollection Add_ProjectsRepositoryDatabase(this IServiceCollection services,
            ServiceToken<ProjectsDbContext> projectsDbContextToken,
            out ServiceToken<IProjectsRepository> projectsRepositoryToken,
            out ServiceToken<IBulkProjectsRepository> bulkProjectsRepositoryToken,
            out ServiceToken<IProjectDescriptorsSearchRepository> projectDescriptorsSearchRepositoryToken)
        {
            Instances.ServicesOperator.Add_DatabaseProjectsRepository(
                services,
                projectsDbContextToken,
                out projectsRepositoryToken,
                out bulkProjectsRepositoryToken,
                out projectDescriptorsSearchRepositoryToken);

            return services;
        }
    }
}
