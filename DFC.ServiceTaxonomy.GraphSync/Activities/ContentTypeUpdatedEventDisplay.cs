using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Workflows.Display;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class ContentTypeUpdatedEventDisplay : ActivityDisplayDriver<ContentTypeUpdated, ContentTypeUpdatedViewModel>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentTypeUpdatedEventDisplay(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }
    }
}
