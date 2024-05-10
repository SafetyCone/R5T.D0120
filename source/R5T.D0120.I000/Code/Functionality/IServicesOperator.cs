using System;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using R5T.H0001;

using R5T.T0132;
using R5T.T0248;


namespace R5T.D0120.I000
{
    [FunctionalityMarker]
    public partial interface IServicesOperator : IFunctionalityMarker
    {
        public void Add_DatabaseProjectsRepository_UsingLocalDevelopment(
            IServiceCollection services,
            out ServiceToken<ProjectsDbContext> projectsDbContexToken,
            out ServiceToken<IProjectsRepository> projectsRepositoryToken,
            out ServiceToken<IBulkProjectsRepository> bulkProjectsRepositoryToken,
            out ServiceToken<IProjectDescriptorsSearchRepository> projectDescriptorsSearchRepositoryToken)
        {
            this.Add_ProjectsDbContext_UsingLocalDevelopment(
                services,
                out projectsDbContexToken);

            this.Add_DatabaseProjectsRepository(
                services,
                projectsDbContexToken,
                out projectsRepositoryToken,
                out bulkProjectsRepositoryToken,
                out projectDescriptorsSearchRepositoryToken);
        }

        public void Add_ProjectsDbContext_UsingLocalDevelopment(
            IServiceCollection services,
            out ServiceToken<ProjectsDbContext> projectsDbContextToken)
        {
            services.AddDbContext<ProjectsDbContext>(options =>
            {
                options.UseSqlServer(Instances.ConnectionStrings.Local_Development);
            });
        }

        public void Add_DatabaseProjectsRepository(
            IServiceCollection services,
            ServiceToken<ProjectsDbContext> projectsDbContextToken,
            out ServiceToken<IProjectsRepository> projectsRepositoryToken,
            out ServiceToken<IBulkProjectsRepository> bulkProjectsRepositoryToken,
            out ServiceToken<IProjectDescriptorsSearchRepository> projectDescriptorsSearchRepositoryToken)
        {
            services.AddScoped<DatabaseProjectsRepository>();
            services.AddScoped<IProjectsRepository>(serviceProvider => serviceProvider.GetService<DatabaseProjectsRepository>());
            services.AddScoped<IBulkProjectsRepository>(serviceProvider => serviceProvider.GetService<DatabaseProjectsRepository>());
            services.AddScoped<IProjectDescriptorsSearchRepository>(serviceProvider => serviceProvider.GetService<DatabaseProjectsRepository>());
        }
    }
}
