using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.ContentApproval.Permissions
{
    public class RequestReviewPermissions : IPermissionProvider
    {
        public static readonly Permission RequestReviewPermission = new Permission(nameof(RequestReviewPermission), "Request review");

        public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
            Task.FromResult(new[]
            {
                RequestReviewPermission,
            }
            .AsEnumerable());

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
            new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = GetPermissionsAsync().Result
                },
            };
    }
}
