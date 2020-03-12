using DFC.ServiceTaxonomy.GraphSync.Activities.Events;
using OrchardCore.Workflows.ViewModels;

namespace DFC.ServiceTaxonomy.GraphSync.ViewModels
{
    public class ContentTypeDeletedEventViewModel : ActivityViewModel<ContentTypeDeletedEvent>
    {
        public ContentTypeDeletedEventViewModel()
        {

        }

        public ContentTypeDeletedEventViewModel(ContentTypeDeletedEvent activity)
        {
            Activity = activity;
        }
    }
}
