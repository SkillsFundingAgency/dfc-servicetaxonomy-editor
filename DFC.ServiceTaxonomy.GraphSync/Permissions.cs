using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.GraphSync
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission AdministerGraphs = new Permission("AdministerGraphs", "Administer graphs");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(GetPermissions());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = GetPermissions()
                }
            };
        }

        private IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                AdministerGraphs
            };
        }
    }
}
