using DFC.ServiceTaxonomy.GraphSync.Activities.Events;
using OrchardCore.Workflows.ViewModels;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class ContentTypeFieldRemovedEventViewModel : ActivityViewModel<ContentTypeFieldRemovedEvent>
    {
        public ContentTypeFieldRemovedEventViewModel()
        {

        }

        public ContentTypeFieldRemovedEventViewModel(ContentTypeFieldRemovedEvent activity)
        {
            Activity = activity;
        }
    }
}

