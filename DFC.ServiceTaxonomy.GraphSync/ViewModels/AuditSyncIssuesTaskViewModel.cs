using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;

namespace DFC.ServiceTaxonomy.Editor.Module.ViewModels
{
    public class AuditSyncIssuesTaskViewModel
    {
        public ValidationScope Scope { get; set; } = ValidationScope.ModifiedSinceLastValidation;
    }
}
