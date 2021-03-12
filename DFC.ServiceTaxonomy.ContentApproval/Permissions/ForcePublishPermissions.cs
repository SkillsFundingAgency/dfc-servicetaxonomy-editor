using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.ContentApproval.Permissions
{
    public class ForcePublishPermissions : IPermissionProvider
    {
        public static readonly Permission ForcePublishPermission = new Permission(nameof(ForcePublishPermission), "Can force publish content");
        public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
            Task.FromResult(new[]
            {
                ForcePublishPermission
            }
            .AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
            new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = GetPermissionsAsync().Result
                }
            };
    }
}
