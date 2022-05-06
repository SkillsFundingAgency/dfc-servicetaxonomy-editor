using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using YesSql.Indexes;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.Indexes;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Indexes
{
    public class JobProfileIndexProvider : ContentHandlerBase, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _partRemoved = new HashSet<string>();
        private IContentDefinitionManager? _contentDefinitionManager;

        public JobProfileIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            var part = context.ContentItem.As<JobProfile>();
            // Validate that the content definition contains this part, this prevents indexing parts
            // that have been removed from the type definition, but are still present in the elements.            
            if (part != null)
            {
                // Lazy initialization because of ISession cyclic dependency.
                _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                // Search for this part.
                var contentTypeDefinition =
                    _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
                if (!contentTypeDefinition.Parts.Any(ctpd => ctpd.Name == nameof(JobProfile)))
                {
                    context.ContentItem.Remove<JobProfile>();
                    _partRemoved.Add(context.ContentItem.ContentItemId);
                }
            }
            return Task.CompletedTask;
        }

        public string? CollectionName { get; set; }
        public Type ForType() => typeof(ContentItem);
        public void Describe(IDescriptor context) => Describe((DescribeContext<ContentItem>)context);

        [return: MaybeNull]
        public void Describe(DescribeContext<ContentItem> context)
        {
            context.For<JobProfileIndex>()
                .When(contentItem => contentItem.Has<JobProfile>() || _partRemoved.Contains(contentItem.ContentItemId))
                .Map(contentItem =>
                {
                    // Remove index records of soft deleted items.
                    if (!contentItem.Published && !contentItem.Latest)
                    {
                        return default!;
                    }

                    var part = contentItem.Content.JobProfile;
                    if (part == null)
                    {
                        return default!;
                    }

                    var jobProfileIndex = new JobProfileIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        GraphSyncPartId = contentItem.As<GraphSyncPart>().ExtractGuid().ToString(),
                        JobProfileTitle = contentItem.DisplayText
                    };

                    jobProfileIndex.DynamicTitlePrefix = GetContentItemListAsCsv(part.DynamicTitlePrefix);
                    jobProfileIndex.JobProfileSpecialism = GetContentItemListAsCsv(part.JobProfileSpecialism);
                    jobProfileIndex.JobProfileCategory = GetContentItemListAsCsv(part.JobProfileCategory);
                    jobProfileIndex.RelatedCareerProfiles = GetContentItemListAsCsv(part.Relatedcareerprofiles);
                    jobProfileIndex.SOCCode = GetContentItemListAsCsv(part.SOCCode);
                    jobProfileIndex.HiddenAlternativeTitle = GetContentItemListAsCsv(part.HiddenAlternativeTitle);
                    jobProfileIndex.WorkingHoursDetail = GetContentItemListAsCsv(part.WorkingHoursDetails);
                    jobProfileIndex.WorkingPatterns = GetContentItemListAsCsv(part.WorkingPattern);
                    jobProfileIndex.WorkingPatternDetail = GetContentItemListAsCsv(part.WorkingPatternDetails);
                    jobProfileIndex.UniversityEntryRequirements = GetContentItemListAsCsv(part.UniversityEntryRequirements);
                    jobProfileIndex.UniversityRequirements = GetContentItemListAsCsv(part.RelatedUniversityRequirements);
                    jobProfileIndex.UniversityLinks = GetContentItemListAsCsv(part.RelatedUniversityLinks);
                    jobProfileIndex.CollegeEntryRequirements = GetContentItemListAsCsv(part.CollegeEntryRequirements);
                    jobProfileIndex.CollegeRequirements = GetContentItemListAsCsv(part.RelatedCollegeRequirements);
                    jobProfileIndex.CollegeLink = GetContentItemListAsCsv(part.RelatedCollegeLinks);
                    jobProfileIndex.ApprenticeshipEntryRequirements = GetContentItemListAsCsv(part.ApprenticeshipEntryRequirements);
                    jobProfileIndex.ApprenticeshipRequirements = GetContentItemListAsCsv(part.RelatedApprenticeshipRequirements);
                    jobProfileIndex.ApprenticeshipLink = GetContentItemListAsCsv(part.RelatedApprenticeshipLinks);
                    jobProfileIndex.Registration = GetContentItemListAsCsv(part.RelatedRegistrations);
                    jobProfileIndex.DigitalSkills = GetContentItemListAsCsv(part.DigitalSkills);
                    jobProfileIndex.RelatedSkills = GetContentItemListAsCsv(part.Relatedskills);
                    jobProfileIndex.Location = GetContentItemListAsCsv(part.RelatedLocations);
                    jobProfileIndex.Environment = GetContentItemListAsCsv(part.RelatedEnvironments);
                    jobProfileIndex.Uniform = GetContentItemListAsCsv(part.RelatedUniforms);
                    jobProfileIndex.Restriction = GetContentItemListAsCsv(part.Relatedrestrictions);

                    return jobProfileIndex;
                });
        }

        private string? GetContentItemListAsCsv(dynamic contentItemIdField)
        {
            if(contentItemIdField == null || contentItemIdField?.ContentItemIds == null)
            {
                return null;
            }

            var contentItemIds = contentItemIdField?.ContentItemIds.ToObject<IList<string>>() as IList<string>;
            return contentItemIds.ConvertListToCsv();
        }
    }
}
