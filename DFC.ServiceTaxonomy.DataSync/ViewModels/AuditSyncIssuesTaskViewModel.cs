using DFC.ServiceTaxonomy.DataSync.Enums;

namespace DFC.ServiceTaxonomy.DataSync.ViewModels
{
    public class AuditSyncIssuesTaskViewModel
    {
        public ValidationScope Scope { get; set; } = ValidationScope.ModifiedSinceLastValidation;
    }
}
