using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using DFC.ServiceTaxonomy.ContentApproval.Permissions;
using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.ContentApproval.Extensions
{
    public static class ReviewTypeExtensions
    {
        public static Permission? GetRelatedPermission(this ReviewType reviewType)
        {
            switch (reviewType)
            {
                case ReviewType.ContentDesign:
                    return CanPerformReviewPermissions.CanPerformContentDesignReviewPermission;
                case ReviewType.SME:
                    return CanPerformReviewPermissions.CanPerformSMEReviewPermission;
                case ReviewType.Stakeholder:
                    return CanPerformApprovalPermissions.CanPerformStakeholderApprovalPermission;
                case ReviewType.UX:
                    return CanPerformReviewPermissions.CanPerformUXReviewPermission;
            }

            return null;
        }
    }
}
