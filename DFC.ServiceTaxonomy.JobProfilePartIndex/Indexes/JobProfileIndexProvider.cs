using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using YesSql.Indexes;
using DFC.ServiceTaxonomy.JobProfilePartIndex.Models;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfilePartIndex.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;

namespace DFC.ServiceTaxonomy.JobProfilePartIndex.Indexes
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

        public void Describe(DescribeContext<ContentItem> context)
        {
            context.For<JobProfileIndex>()
                .When(contentItem => contentItem.Has<JobProfile>() || _partRemoved.Contains(contentItem.ContentItemId))
                .Map(contentItem =>
                {
                    // Remove index records of soft deleted items.
                    if (!contentItem.Published && !contentItem.Latest)
                    {
                        return null;
                    }

                    var part = contentItem.Content.JobProfile;
                    if (part == null)
                    {
                        return null;
                    }

                    return  new JobProfileIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        GraphSyncPartId = (contentItem.As<GraphSyncPart>()).ExtractGuid().ToString(),
                        DynamicTitlePrefix = (part.Dynamictitleprefix.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        JobProfileSpecialism = (part.Jobprofilespecialism.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        JobProfileCategory = (part.Jobprofilecategory.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        RelatedCareerProfiles = (part.Relatedcareerprofiles.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        SOCCode = (part.SOCcode.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        HiddenAlternativeTitle = (part.HiddenAlternativeTitle.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        WorkingHoursDetail = (part.WorkingHoursDetails.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        WorkingPatterns = (part.Workingpattern.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        WorkingPatternDetail = (part.Workingpatterndetails.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        UniversityEntryRequirements = (part.Universityentryrequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        UniversityRequirements = (part.Relateduniversityrequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        UniversityLinks = (part.Relateduniversitylinks.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        CollegeentryRequirements = (part.Collegeentryrequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        CollegeRequirements = (part.Relatedcollegerequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        CollegeLink = (part.Relatedcollegelinks.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        ApprenticeshipEntryRequirements = (part.Apprenticeshipentryrequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        ApprenticeshipRequirements = (part.Relatedapprenticeshiprequirements.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        ApprenticeshipLink = (part.Relatedapprenticeshiplinks.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        Registration = (part.Relatedregistrations.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        Digitalskills = (part.Digitalskills.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        Location = (part.Relatedlocations.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        Environment = (part.Relatedenvironments.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                        Uniform = (part.Relateduniforms.ContentItemIds.ToObject<IList<string>>() as IList<string>).ConvertListToCsv(),
                    };
                });
        }
    }
}
