using System;
using System.Threading.Tasks;

using R5T.T0238;


namespace R5T.D0120.I000
{
    public static class IProjectDescriptorsSearchRepositoryExtensions
    {
        public static Task<ProjectDescriptor[]> Find_ByNameContainsText(this IProjectDescriptorsSearchRepository repository,
            string searchText)
            => repository.Find_By(x => x.Name.Contains(searchText));
    }
}
