using OrchardCore.Workflows.ViewModels;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class ContentTypeUpdatedViewModel : ActivityViewModel<ContentTypeUpdatedEvent>
    {
        public ContentTypeUpdatedViewModel()
        {

        }

        public ContentTypeUpdatedViewModel(ContentTypeUpdatedEvent activity)
        {
            Activity = activity;
        }
    }
}

