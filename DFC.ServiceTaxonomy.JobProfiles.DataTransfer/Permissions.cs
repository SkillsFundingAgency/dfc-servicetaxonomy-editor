using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAzureSearchIndex = new Permission(nameof(ManageAzureSearchIndex), "Manage Azure indexing");

        public Task<IEnumerable<Permission>> GetPermissionsAsync() => Task.FromResult(new[] { ManageAzureSearchIndex }.AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() => new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageAzureSearchIndex }
                }
            };
    }
}
