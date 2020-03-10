using DFC.ServiceTaxonomy.GraphSync.Activities;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.GraphSync.Drivers.Events
{
    public class DeleteContentTypeFromGraphTaskDisplay : ActivityDisplayDriver<DeleteContentTypeFromGraphTask, DeleteContentTypeFromGraphTaskViewModel>
    {
        protected override void EditActivity(DeleteContentTypeFromGraphTask activity, DeleteContentTypeFromGraphTaskViewModel model)
        {
            //model.Value = activity.Value.Expression;
        }

        protected override void UpdateActivity(DeleteContentTypeFromGraphTaskViewModel model, DeleteContentTypeFromGraphTask activity)
        {
            //activity.Value = new WorkflowExpression<string>(model.Value);
        }
    }
}
