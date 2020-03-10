using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using DFC.ServiceTaxonomy.GraphSync.Activities;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    public class DeleteContentTypeTaskDisplay : ActivityDisplayDriver<DeleteContentTypeTask, DeleteContentTypeTaskViewModel>
    {
        protected override void EditActivity(DeleteContentTypeTask activity, DeleteContentTypeTaskViewModel model)
        {
            //model.Value = activity.Value.Expression;
        }

        protected override void UpdateActivity(DeleteContentTypeTaskViewModel model, DeleteContentTypeTask activity)
        {
            //activity.Value = new WorkflowExpression<string>(model.Value);
        }
    }
}
