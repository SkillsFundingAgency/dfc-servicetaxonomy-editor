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
                .When(contentItem => string.Equals("jobProfileCategory", contentItem.ContentType, StringComparison.OrdinalIgnoreCase)
                || string.Equals("jobProfile", contentItem.ContentType, StringComparison.OrdinalIgnoreCase))
                .Map(contentItem =>
                {
                    // Remove index records of soft deleted items.
                    if (!contentItem.Published && !contentItem.Latest)
                    {
                        return default!;
                    }
                    return JobProfileCategoryPartAsync(contentItem);
                });
        }

        public List<JobProfileCategoriesPartIndex> JobProfileCategoryPartAsync(ContentItem contentItem)
        {
            if ((contentItem.Published || contentItem.Latest))
            {
                List<string> jobCategoryItemIds = new List<string>();
                List<JobProfileCategoriesPartIndex> indexes = new List<JobProfileCategoriesPartIndex>();

                if (string.Equals("jobProfile", contentItem.ContentType, StringComparison.OrdinalIgnoreCase))
                {
                    var content = JsonConvert.SerializeObject(contentItem.Content);

                    var root = JToken.Parse(content);
                    jobCategoryItemIds = root.SelectToken("..JobProfileCategories.ContentItemIds").ToObject<List<string>>();
                }
                else
                {
                    jobCategoryItemIds.Add(contentItem.ContentItemId);
                }

                jobCategoryItemIds.ForEach(async delegate (string categoryItemId) {
                    var relatedProfiles = await contentItemService.GetReferencingContentItems(categoryItemId);

                    var profileIds = relatedProfiles.ToList();

                    indexes.Add(new JobProfileCategoriesPartIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        RelatedJobProfileContentItemIds = string.Join(',', profileIds.Select(w => w.ContentItemId)),
                    });
                });
                return indexes;
            }

#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    
}
