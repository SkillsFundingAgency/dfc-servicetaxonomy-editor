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
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.Indexes;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Indexes
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

                    return  new JobProfileIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        GraphSyncPartId = (contentItem.As<GraphSyncPart>()).ExtractGuid().ToString(),
                        DynamicTitlePrefix = (part.DynamicTitlePrefix.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        JobProfileSpecialism = (part.JobProfileSpecialism.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        JobProfileCategory = (part.JobProfileCategory.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        RelatedCareerProfiles = (part.Relatedcareerprofiles.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        SOCCode = (part.SOCCode.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        HiddenAlternativeTitle = (part.HiddenAlternativeTitle.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        WorkingHoursDetail = (part.WorkingHoursDetails.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        WorkingPatterns = (part.WorkingPattern.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        WorkingPatternDetail = (part.WorkingPatternDetails.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        UniversityEntryRequirements = (part.UniversityEntryRequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        UniversityRequirements = (part.RelatedUniversityRequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        UniversityLinks = (part.RelatedUniversityLinks.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        CollegeEntryRequirements = (part.CollegeEntryRequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        CollegeRequirements = (part.RelatedCollegeRequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        CollegeLink = (part.RelatedCollegeLinks.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        ApprenticeshipEntryRequirements = (part.ApprenticeshipEntryRequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        ApprenticeshipRequirements = (part.RelatedApprenticeshipRequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        ApprenticeshipLink = (part.RelatedApprenticeshipLinks.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        Registration = (part.RelatedRegistrations.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        DigitalSkills = (part.DigitalSkills.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        Location = (part.RelatedLocations.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        Environment = (part.RelatedEnvironments.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        Uniform = (part.RelatedUniforms.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        JobProfileTitle = contentItem.DisplayText,
                        Restriction = (part.Relatedrestrictions.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                    };
                });
        }
    }
}
