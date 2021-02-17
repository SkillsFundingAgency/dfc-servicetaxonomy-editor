using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.ContentApproval.Permissions
{
    public class CanPerformReviewPermissions : IPermissionProvider
    {
        public static readonly Permission CanPerformContentDesignReviewPermission = new Permission(nameof(CanPerformContentDesignReviewPermission), "Can perform content design review");
        public static readonly Permission CanPerformSMEReviewPermission = new Permission(nameof(CanPerformSMEReviewPermission), "Can perform SME review");
        public static readonly Permission CanPerformUXReviewPermission = new Permission(nameof(CanPerformUXReviewPermission), "Can perform UX review");
        public static readonly Permission CanPerformReviewPermission = new Permission(nameof(CanPerformUXReviewPermission), "Can perform UX review", new []{CanPerformUXReviewPermission, CanPerformSMEReviewPermission, CanPerformContentDesignReviewPermission});

        public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
            Task.FromResult(new[]
            {
                CanPerformContentDesignReviewPermission,
                CanPerformSMEReviewPermission,
                CanPerformUXReviewPermission
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
