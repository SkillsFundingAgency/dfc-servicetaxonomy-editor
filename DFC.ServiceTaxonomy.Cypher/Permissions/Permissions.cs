using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.Cypher.Permissions
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageCypherQueries = new Permission("ManageCypherQueries", "Manage Cypher Queries");

        public Task<IEnumerable<Permission>> GetPermissionsAsync() => Task.FromResult(new[]
            {
                ManageCypherQueries
            }
            .AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() => new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageCypherQueries }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { ManageCypherQueries }
                }
            };
    }
}
