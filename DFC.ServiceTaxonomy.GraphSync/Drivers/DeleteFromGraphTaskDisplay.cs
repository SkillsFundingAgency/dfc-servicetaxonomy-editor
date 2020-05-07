using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using DFC.ServiceTaxonomy.GraphSync.Activities;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.GraphSync.Drivers
{
    public class DeleteFromGraphTaskDisplay : ActivityDisplayDriver<DeleteFromGraphTask, DeleteFromGraphTaskViewModel>
    {
        protected override void EditActivity(DeleteFromGraphTask activity, DeleteFromGraphTaskViewModel model)
        {
            //model.Value = activity.Value.Expression;
        }

        protected override void UpdateActivity(DeleteFromGraphTaskViewModel model, DeleteFromGraphTask activity)
        {
            //activity.Value = new WorkflowExpression<string>(model.Value);
        }
    }
}
