using DFC.ServiceTaxonomy.DataSync.Activities;
using DFC.ServiceTaxonomy.DataSync.ViewModels;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.DataSync.Drivers
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
