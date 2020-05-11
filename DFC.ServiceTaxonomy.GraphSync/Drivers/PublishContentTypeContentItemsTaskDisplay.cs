using DFC.ServiceTaxonomy.GraphSync.Activities;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.GraphSync.Drivers
{
    public class PublishContentTypeContentItemsTaskDisplay : ActivityDisplayDriver<PublishContentTypeContentItemsTask, PublishContentTypeContentItemsTaskViewModel>
    {
        protected override void EditActivity(PublishContentTypeContentItemsTask activity, PublishContentTypeContentItemsTaskViewModel model)
        {
            //model.Value = activity.Value.Expression;
        }

        protected override void UpdateActivity(PublishContentTypeContentItemsTaskViewModel model, PublishContentTypeContentItemsTask activity)
        {
            //activity.Value = new WorkflowExpression<string>(model.Value);
        }
    }
}
