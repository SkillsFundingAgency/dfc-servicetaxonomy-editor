using DFC.ServiceTaxonomy.GraphSync.Activities;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    public class RemoveFieldFromContentItemsTaskDisplay : ActivityDisplayDriver<RemoveFieldFromContentItemsTask, RemoveFieldFromContentItemsTaskViewModel>
    {
        protected override void EditActivity(RemoveFieldFromContentItemsTask activity, RemoveFieldFromContentItemsTaskViewModel model)
        {
            //model.Value = activity.Value.Expression;
        }

        protected override void UpdateActivity(RemoveFieldFromContentItemsTaskViewModel model, RemoveFieldFromContentItemsTask activity)
        {
            //activity.Value = new WorkflowExpression<string>(model.Value);
        }
    }
}
