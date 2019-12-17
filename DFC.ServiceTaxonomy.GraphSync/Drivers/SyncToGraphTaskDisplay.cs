using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using DFC.ServiceTaxonomy.GraphSync.Activities;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    public class SyncToGraphTaskDisplay : ActivityDisplayDriver<SyncToGraphTask, SyncToGraphTaskViewModel>
    {
        protected override void EditActivity(SyncToGraphTask activity, SyncToGraphTaskViewModel model)
        {
            //model.Value = activity.Value.Expression;
        }

        protected override void UpdateActivity(SyncToGraphTaskViewModel model, SyncToGraphTask activity)
        {
            //activity.Value = new WorkflowExpression<string>(model.Value);
        }
    }
}
