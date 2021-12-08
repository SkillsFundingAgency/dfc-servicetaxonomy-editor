using DFC.ServiceTaxonomy.GraphSync.Enums;

namespace DFC.ServiceTaxonomy.Editor.Module.ViewModels
{
    public class AuditSyncIssuesTaskViewModel
    {
        public ValidationScope Scope { get; set; } = ValidationScope.ModifiedSinceLastValidation;
    }
}
