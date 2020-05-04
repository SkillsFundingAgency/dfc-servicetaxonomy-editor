using DFC.ServiceTaxonomy.Editor.Activities.Tasks;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.Editor.Drivers
{
    //todo: this for other tasks displays
    //todo: edit & design & thumbnails view should use .
    public class PublishToEventGridTaskDisplay : ActivityDisplayDriver<PublishToEventGridTask>
    {
    }

    // public class PublishToEventGridTaskDisplay : ActivityDisplayDriver<PublishToEventGridTask, PublishToEventGridTaskViewModel>
    // {
    //     protected override void EditActivity(PublishToEventGridTask activity, PublishToEventGridTaskViewModel model)
    //     {
    //         //model.Value = activity.Value.Expression;
    //     }
    //
    //     protected override void UpdateActivity(PublishToEventGridTaskViewModel model, PublishToEventGridTask activity)
    //     {
    //         //activity.Value = new WorkflowExpression<string>(model.Value);
    //     }
    // }
}
