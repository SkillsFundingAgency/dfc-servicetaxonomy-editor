using DFC.ServiceTaxonomy.GraphSync.Activities;
using DFC.ServiceTaxonomy.GraphSync.ViewModels;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    public class CreateSynonymsTaskDisplay : ActivityDisplayDriver<CreateSynonymsTask, CreateSynonymsTaskViewModel>
    {
        protected override void EditActivity(CreateSynonymsTask activity, CreateSynonymsTaskViewModel model)
        {
            //model.Value = activity.Value.Expression;
        }

        protected override void UpdateActivity(CreateSynonymsTaskViewModel model, CreateSynonymsTask activity)
        {
            //activity.Value = new WorkflowExpression<string>(model.Value);
        }
    }
}
