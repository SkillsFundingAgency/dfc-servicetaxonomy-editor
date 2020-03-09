using DFC.ServiceTaxonomy.GraphSync.Activities;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    public class GetContentItemsTaskDisplay : ActivityDisplayDriver<GetContentItemsTask, GetContentItemsTaskViewModel>
    {
        protected override void EditActivity(GetContentItemsTask activity, GetContentItemsTaskViewModel model)
        {
            //model.Value = activity.Value.Expression;
        }

        protected override void UpdateActivity(GetContentItemsTaskViewModel model, GetContentItemsTask activity)
        {
            //activity.Value = new WorkflowExpression<string>(model.Value);
        }
    }
}
