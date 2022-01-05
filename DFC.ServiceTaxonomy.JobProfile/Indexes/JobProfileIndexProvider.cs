using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.JobProfile.Models;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.JobProfile.Indexes
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
            var part = context.ContentItem.As<JobProfilePart>();
            // Validate that the content definition contains this part, this prevents indexing parts
            // that have been removed from the type definition, but are still present in the elements.            
            if (part != null)
            {
                // Lazy initialization because of ISession cyclic dependency.
                _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                // Search for this part.
                var contentTypeDefinition =
                    _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
                if (!contentTypeDefinition.Parts.Any(ctpd => ctpd.Name == nameof(JobProfilePart)))
                {
                    context.ContentItem.Remove<JobProfilePart>();
                    _partRemoved.Add(context.ContentItem.ContentItemId);
                }
            }
            return Task.CompletedTask;
        }

        public string? CollectionName { get; set; }
        public Type ForType() => typeof(ContentItem);
        public void Describe(IDescriptor context) => Describe((DescribeContext<ContentItem>)context);

        public void Describe(DescribeContext<ContentItem> context)
        {
            var temp = context.For<JobProfileIndex>()
                .When(contentItem => contentItem.Has<JobProfilePart>() || _partRemoved.Contains(contentItem.ContentItemId));
            context.For<JobProfileIndex>()
                .When(contentItem => contentItem.Has<JobProfilePart>() || _partRemoved.Contains(contentItem.ContentItemId))
                .Map(contentItem =>
                {
                    // Remove index records of soft deleted items.
                    if (!contentItem.Published && !contentItem.Latest)
                    {
                        return null;
                    }

                    var part = contentItem.As<JobProfilePart>();
                    if (part == null)
                    {
                        return null;
                    }

                    return new JobProfileIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        DynamicTitlePrefix = part.Dynamictitleprefix,
                        JobProfileSpecialism = part.Jobprofilespecialism,
                        JobProfileCategory = part.Jobprofilecategory,
                        RelatedCareerProfiles = part.Relatedcareerprofiles,
                        SOCCode = part.SOCcode,
                        HiddenAlternativeTitle = part.HiddenAlternativeTitle,
                        WorkingHoursDetail = part.WorkingHoursDetails,
                        WorkingPatterns = part.Workingpattern,
                        WorkingPatternDetail = part.Workingpatterndetails,
                        UniversityEntryRequirements = part.Universityentryrequirements,
                        UniversityRequirements = part.Relateduniversityrequirements,
                        UniversityLinks = part.Relateduniversitylinks,
                        CollegeentryRequirements = part.Collegeentryrequirements,
                        CollegeRequirements = part.Relatedcollegerequirements,
                        CollegeLink = part.Relatedcollegelinks,
                        ApprenticeshipEntryRequirements = part.Apprenticeshipentryrequirements,
                        ApprenticeshipRequirements = part.Relatedapprenticeshiprequirements,
                        ApprenticeshipLink = part.Relatedapprenticeshiplinks,
                        Registration = part.Relatedregistrations,
                        Digitalskills = part.Digitalskills,
                        Location = part.Relatedlocations,
                        Environment = part.Relatedenvironments,
                        Uniform = part.Relateduniforms,
                    };
                });
        }
    }
}
