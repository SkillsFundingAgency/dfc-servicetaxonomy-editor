using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.ContentApproval.Permissions
{
    public class CanPerformApprovalPermissions : IPermissionProvider
    {
        public static readonly Permission CanPerformStakeholderApprovalPermission = new Permission(nameof(CanPerformStakeholderApprovalPermission), "Can perform stakeholder approval");

        public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
            Task.FromResult(new[]
                {
                    CanPerformStakeholderApprovalPermission
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
