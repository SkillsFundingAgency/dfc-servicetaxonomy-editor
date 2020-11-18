using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using DFC.ServiceTaxonomy.GraphSync.Activities;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    public class AuditSyncIssuesTaskDisplay : ActivityDisplayDriver<AuditSyncIssuesTask, AuditSyncIssuesTaskViewModel>
    {
        protected override void EditActivity(AuditSyncIssuesTask activity, AuditSyncIssuesTaskViewModel model)
        {
            model.Scope = activity.Scope;
        }

        protected override void UpdateActivity(AuditSyncIssuesTaskViewModel model, AuditSyncIssuesTask activity)
        {
            activity.Scope = model.Scope;
        }
    }
}
