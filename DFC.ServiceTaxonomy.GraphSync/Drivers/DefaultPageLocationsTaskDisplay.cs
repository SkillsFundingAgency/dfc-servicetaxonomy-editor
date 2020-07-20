using DFC.ServiceTaxonomy.GraphSync.Activities;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.GraphSync.Drivers
{
    public class DefaultPageLocationsTaskDisplay : ActivityDisplayDriver<DefaultPageLocationsTask, PublishContentTypeContentItemsTaskViewModel>
    {
        protected override void EditActivity(DefaultPageLocationsTask activity, PublishContentTypeContentItemsTaskViewModel model)
        {
            //model.Value = activity.Value.Expression;
        }

        protected override void UpdateActivity(PublishContentTypeContentItemsTaskViewModel model, DefaultPageLocationsTask activity)
        {
            //activity.Value = new WorkflowExpression<string>(model.Value);
        }
    }
}
