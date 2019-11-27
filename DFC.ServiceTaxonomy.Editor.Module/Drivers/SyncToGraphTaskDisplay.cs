using DFC.ServiceTaxonomy.Editor.Module.Activities;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

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