using DFC.ServiceTaxonomy.Dysac.Indexes;
using DFC.ServiceTaxonomy.Dysac.Models;
using DFC.ServiceTaxonomy.Dysac.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using YesSql.Indexes;


namespace DFC.ServiceTaxonomy.Dysac.Indexes
{

    public class JobProfileCategoriesPartIndexProvider : IndexProvider<ContentItem>
    {
        private readonly ILogger logger;
        private readonly IContentItemService contentItemService;
        public JobProfileCategoriesPartIndexProvider(
            ILogger<JobProfileCategoriesPartIndexProvider> logger,
            IContentItemService contentItemService)
        {
            this.logger = logger;
            this.contentItemService = contentItemService;
        }
        public override void Describe(DescribeContext<ContentItem> context)
        {

            context.For<JobProfileCategoriesPartIndex>()
                .When(contentItem => string.Equals("jobProfileCategory", contentItem.ContentType, StringComparison.OrdinalIgnoreCase))
                .Map(contentItem =>
                {
                   return JobProfileCategoryPartAsync(contentItem);
                });
        }

        public async Task<JobProfileCategoriesPartIndex> JobProfileCategoryPartAsync(ContentItem contentItem)
        {
            if ((contentItem.Published || contentItem.Latest))
            {
                // Remove index records of soft deleted items.
                if (!contentItem.Published && !contentItem.Latest)
                {
                    return default!;
                }


                var relatedItems = await contentItemService.GetReferencingContentItems(contentItem.ContentItemId);

                /*var index = new JobProfileCategoriesPartIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    RelatedJobProfileContentItemIds = string.Join(',', tiles),
                };
                return index;*/
            }

#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    
}
