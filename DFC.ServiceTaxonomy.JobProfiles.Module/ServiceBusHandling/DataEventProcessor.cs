using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Indexes;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;

using Microsoft.Extensions.Logging;

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

using DFC.ServiceTaxonomy.DataAccess.Repositories;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling
{
    internal class DataEventProcessor : IDataEventProcessor
    {
        private readonly IServiceBusMessageProcessor _serviceBusMessageProcessor;
        private readonly ILogger<DataEventProcessor> _logger;
        private readonly IMessageConverter<JobProfileMessage> _jobprofileMessageConverter;
        private readonly IRelatedSkillsConverter _relatedSkillsConverter;
        private readonly IGenericIndexRepository<JobProfileIndex> _jobProfileIndexRepository;

public DataEventProcessor(IServiceBusMessageProcessor serviceBusMessageProcessor,
                                    ILogger<DataEventProcessor> logger,
                                    IMessageConverter<JobProfileMessage> jobprofileMessageConverter,
                                    IRelatedSkillsConverter relatedSkillsConverter,
                                    IGenericIndexRepository<JobProfileIndex> jobProfileIndexRepository)
        {
            _serviceBusMessageProcessor = serviceBusMessageProcessor;
            _logger = logger;
            _jobprofileMessageConverter = jobprofileMessageConverter;
            _relatedSkillsConverter = relatedSkillsConverter;
            _jobProfileIndexRepository = jobProfileIndexRepository;
        }

        public async Task ProcessContentContext(ContentContextBase context, string actionType)
        {
            try
            {
                switch (context.ContentItem.ContentType)
                {
                    case ContentTypes.JobProfile:
                        await GenerateServiceBusMessageForJobProfile(context, actionType);
                        break;

                    case ContentTypes.Environment:
                    case ContentTypes.Uniform:
                    case ContentTypes.Location:
                        await GenerateServiceBusMessageForWYDTypes(context, actionType);
                        break;

                    case ContentTypes.UniversityLink:
                    case ContentTypes.CollegeLink:
                    case ContentTypes.ApprenticeshipLink:
                        await GenerateServiceBusMessageForTextFieldTypes(context, actionType);
                        break;

                    case ContentTypes.Restriction:
                    case ContentTypes.Registration:
                    case ContentTypes.ApprenticeshipRequirements:
                    case ContentTypes.CollegeRequirements:
                    case ContentTypes.UniversityRequirements:
                        await GenerateServiceBusMessageForInfoTypes(context, actionType);
                        break;

                    case ContentTypes.WorkingHoursDetail:
                    case ContentTypes.WorkingPatterns:
                    case ContentTypes.HiddenAlternativeTitle:
                    case ContentTypes.WorkingPatternDetail:
                    case ContentTypes.UniversityEntryRequirements:
                    case ContentTypes.CollegeEntryRequirements:
                    case ContentTypes.JobProfileSpecialism:
                    case ContentTypes.ApprenticeshipEntryRequirements:
                        await GenerateServiceBusMessageForOtherReferenceTypes(context, actionType);
                        break;

                    case ContentTypes.SOCSkillsMatrix:
                        await GenerateServiceBusMessageForSocSkillsMatrixType(context, actionType);
                        break;

                    //case ContentTypes.JobProfileSpecialism:
                    // apprenticeship-standards	
                    // apprenticeship-frameworks
                    // HiddenAlternativeTitle
                    // UniversityEntryRequirements
                    // CollegeEntryRequirements
                    // WorkingHoursDetails
                    // WorkingHoursDetails
                    // WorkingPatternDetails
                    // 
                    //break;

                    default:
                        _logger.LogInformation($"Content type '{context.ContentItem.ContentType}' for this item with ContentItemId = '{context.ContentItem.ContentItemId}'  is not proccessed for ServiceBus messaging");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to export data for item with ContentItemId = {context.ContentItem.ContentItemId}");
                throw;
            }
        }

        private async Task GenerateServiceBusMessageForJobProfile(ContentContextBase context, string actionType)
        {
            bool isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            var jobprofileMessage = isSaved
                ? await _jobprofileMessageConverter.ConvertFromAsync(context.ContentItem)
                : new JobProfileMessage()
                {
                    JobProfileId = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                    Title = context.ContentItem.Content.Title
                };

            await _serviceBusMessageProcessor.SendJobProfileMessage(jobprofileMessage, context.ContentItem.ContentType, actionType);
        }

        private async Task GenerateServiceBusMessageForWYDTypes(ContentContextBase context, string actionType)
        {
            bool isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            IList<WhatYouWillDoContentItem> jobprofileData = new List<WhatYouWillDoContentItem>();
            string fieldDescription = context.ContentItem.ContentType switch
            {
                ContentTypes.Environment => Helper.SanitiseHtml(context.ContentItem.Content.Environment.Description.Html),
                ContentTypes.Uniform => Helper.SanitiseHtml(context.ContentItem.Content.Uniform.Description.Html),
                ContentTypes.Location => Helper.SanitiseHtml(context.ContentItem.Content.Location.Description.Html),
                _ => throw new ArgumentException("No valid match found"),
            };

            var matches = await _jobProfileIndexRepository.GetAll(b => b.Uniform != null && b.Uniform.Contains(context.ContentItem.ContentItemId) ||
                b.Environment != null && b.Environment.Contains(context.ContentItem.ContentItemId) ||
                b.Location != null && b.Location.Contains(context.ContentItem.ContentItemId)).ListAsync();

            foreach (var item in matches)
            {
                if (isSaved)
                {
                    jobprofileData.Add(new WhatYouWillDoContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = context.ContentItem.Content.TitlePart.Title,
                        Description = fieldDescription,
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty),
                        JobProfileTitle = item.JobProfileTitle,
                        Url = Helper.GetSlugValue(fieldDescription),
                        IsNegative = false
                    });
                }
                else
                {
                    jobprofileData.Add(new WhatYouWillDoContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty)
                    });

                }
            }

            await _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, context.ContentItem.ContentType, actionType);
        }

        private async Task GenerateServiceBusMessageForTextFieldTypes(ContentContextBase context, string actionType)
        {
            bool isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            IList<TextFieldContentItem> jobprofileData = new List<TextFieldContentItem>();
            string fieldText = context.ContentItem.ContentType switch
            {
                ContentTypes.UniversityLink => context.ContentItem.Content.UniversityLink.Text.Text,
                ContentTypes.CollegeLink => context.ContentItem.Content.CollegeLink.Text.Text,
                ContentTypes.ApprenticeshipLink => context.ContentItem.Content.ApprenticeshipLink.Text.Text,
                _ => throw new ArgumentException("No valid match found"),
            };

            string fieldUrl = context.ContentItem.ContentType switch
            {
                ContentTypes.UniversityLink => context.ContentItem.Content.UniversityLink.URL.Text,
                ContentTypes.CollegeLink => context.ContentItem.Content.CollegeLink.URL.Text,
                ContentTypes.ApprenticeshipLink => context.ContentItem.Content.ApprenticeshipLink.URL.Text,
                _ => throw new ArgumentException("No valid match found"),
            };

            var matches = await _jobProfileIndexRepository.GetAll(b => b.UniversityLinks != null && b.UniversityLinks.Contains(context.ContentItem.ContentItemId) ||
                b.CollegeLink != null && b.CollegeLink.Contains(context.ContentItem.ContentItemId) ||
                b.ApprenticeshipLink != null && b.ApprenticeshipLink.Contains(context.ContentItem.ContentItemId)).ListAsync();

            foreach (var item in matches)
            {
                if (isSaved)
                {
                    jobprofileData.Add(new TextFieldContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = context.ContentItem.DisplayText,
                        Text = fieldText,
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty),
                        JobProfileTitle = item.JobProfileTitle,
                        Url = fieldUrl, 
                    });
                }
                else
                {
                    jobprofileData.Add(new TextFieldContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty)
                    });

                }
            }

            await _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, context.ContentItem.ContentType, actionType);
        }

        private async Task GenerateServiceBusMessageForInfoTypes(ContentContextBase context, string actionType)
        {

            bool isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            IList<InfoContentItem> jobprofileData = new List<InfoContentItem>();
            string fieldInfo = context.ContentItem.ContentType switch
            {
                ContentTypes.Restriction => Helper.SanitiseHtml(context.ContentItem.Content.Restriction.Info.Html),
                ContentTypes.Registration => Helper.SanitiseHtml(context.ContentItem.Content.Registration.Info.Html),
                ContentTypes.ApprenticeshipRequirements => Helper.SanitiseHtml(context.ContentItem.Content.ApprenticeshipRequirements.Info.Html),
                ContentTypes.CollegeRequirements => Helper.SanitiseHtml(context.ContentItem.Content.CollegeRequirements.Info.Html),
                ContentTypes.UniversityRequirements => Helper.SanitiseHtml(context.ContentItem.Content.UniversityRequirements.Info.Html),
                _ => throw new ArgumentException("No valid match found"),
            };


            var matches = await _jobProfileIndexRepository.GetAll(b => b.Restriction != null && b.Restriction.Contains(context.ContentItem.ContentItemId) ||
                b.Registration != null && b.Registration.Contains(context.ContentItem.ContentItemId) ||
                b.ApprenticeshipRequirements != null && b.ApprenticeshipRequirements.Contains(context.ContentItem.ContentItemId) ||
                b.CollegeRequirements != null && b.CollegeRequirements.Contains(context.ContentItem.ContentItemId) ||
                b.UniversityRequirements != null && b.UniversityRequirements.Contains(context.ContentItem.ContentItemId)).ListAsync();

            foreach (var item in matches)
            {
                if (isSaved)
                {
                    jobprofileData.Add(new InfoContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = context.ContentItem.DisplayText,
                        Info = fieldInfo,
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty),
                        JobProfileTitle = item.JobProfileTitle,
                    });
                }
                else
                {
                    jobprofileData.Add(new InfoContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty)
                    });

                }
            }

            await _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, context.ContentItem.ContentType, actionType);

        }

        private async Task GenerateServiceBusMessageForOtherReferenceTypes(ContentContextBase context, string actionType)
        {
            bool isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            IList<OtherReferenceFieldContentItem> jobprofileData = new List<OtherReferenceFieldContentItem>();
            string fieldDescription = context.ContentItem.ContentType switch
            {
                ContentTypes.WorkingHoursDetail => context.ContentItem.Content.WorkingHoursDetail.Description.Text,
                ContentTypes.WorkingPatterns => context.ContentItem.Content.WorkingPatterns.Description.Text,
                ContentTypes.HiddenAlternativeTitle => context.ContentItem.Content.HiddenAlternativeTitle.Description.Text,
                ContentTypes.WorkingPatternDetail => context.ContentItem.Content.WorkingPatternDetail.Description.Text,
                ContentTypes.UniversityEntryRequirements => context.ContentItem.Content.UniversityEntryRequirements.Description.Text,
                ContentTypes.CollegeEntryRequirements => context.ContentItem.Content.CollegeEntryRequirements.Description.Text,
                ContentTypes.JobProfileSpecialism => context.ContentItem.Content.JobProfileSpecialism.Description.Text,
                ContentTypes.ApprenticeshipEntryRequirements => context.ContentItem.Content.ApprenticeshipEntryRequirements.Description.Text,
                _ => throw new ArgumentException("No valid match found"),
            };

            string urlString = context.ContentItem.ContentType switch
            {
                ContentTypes.WorkingHoursDetail => Helper.GetHyphenated((string)context.ContentItem.Content.TitlePart.Title),
                ContentTypes.WorkingPatterns => Helper.GetHyphenated((string)context.ContentItem.Content.TitlePart.Title),
                ContentTypes.HiddenAlternativeTitle => Helper.GetHyphenated((string)context.ContentItem.Content.TitlePart.Title),
                ContentTypes.WorkingPatternDetail => Helper.GetHyphenated((string)context.ContentItem.Content.TitlePart.Title),
                ContentTypes.UniversityEntryRequirements => Helper.GetHyphenated((string)context.ContentItem.Content.TitlePart.Title),
                ContentTypes.CollegeEntryRequirements => Helper.GetHyphenated((string)context.ContentItem.Content.TitlePart.Title),
                ContentTypes.JobProfileSpecialism => Helper.GetHyphenated((string)context.ContentItem.Content.TitlePart.Title),
                ContentTypes.ApprenticeshipEntryRequirements => Helper.GetHyphenated((string)context.ContentItem.Content.TitlePart.Title),
                _ => throw new ArgumentException("No valid match found"),
            };

            var matches = await _jobProfileIndexRepository.GetAll(b => b.WorkingHoursDetail != null && b.WorkingHoursDetail.Contains(context.ContentItem.ContentItemId) ||
                b.WorkingPatterns != null && b.WorkingPatterns.Contains(context.ContentItem.ContentItemId) ||
                b.HiddenAlternativeTitle != null && b.HiddenAlternativeTitle.Contains(context.ContentItem.ContentItemId) ||
                b.WorkingPatternDetail != null && b.WorkingPatternDetail.Contains(context.ContentItem.ContentItemId) ||
                b.UniversityEntryRequirements != null && b.UniversityEntryRequirements.Contains(context.ContentItem.ContentItemId) ||
                b.CollegeEntryRequirements != null && b.CollegeEntryRequirements.Contains(context.ContentItem.ContentItemId) ||
                b.JobProfileSpecialism != null && b.JobProfileSpecialism.Contains(context.ContentItem.ContentItemId) ||
                b.ApprenticeshipEntryRequirements != null && b.ApprenticeshipEntryRequirements.Contains(context.ContentItem.ContentItemId)).ListAsync();

            foreach (var item in matches)
            {
                if (isSaved)
                {
                    jobprofileData.Add(new OtherReferenceFieldContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        Title = context.ContentItem.Content.TitlePart.Title,
                        Description = fieldDescription,
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty),
                        JobProfileTitle = item.JobProfileTitle,
                        Url = urlString, 
                    });
                }
                else
                {
                    jobprofileData.Add(new OtherReferenceFieldContentItem()
                    {
                        Id = context.ContentItem.As<GraphSyncPart>().ExtractGuid(),
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty)
                    });

                }
            }

            await _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, context.ContentItem.ContentType, actionType);
        }

        private async Task GenerateServiceBusMessageForSocSkillsMatrixType(ContentContextBase context, string actionType)
        {
            var isSaved = actionType.Equals(ActionTypes.Published) || actionType.Equals(ActionTypes.Draft);
            var matches = await _jobProfileIndexRepository.GetAll(b => b.RelatedSkills != null && b.RelatedSkills.Contains(context.ContentItem.ContentItemId)).ListAsync();
            var id = context.ContentItem.As<GraphSyncPart>().ExtractGuid();
            var jobprofileData = new List<SocSkillMatrixContentItem>();
            if (!isSaved)
            {
                jobprofileData.AddRange(matches
                    .Select(item => new SocSkillMatrixContentItem {
                        Id = id,
                        JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty)
                    }));
            }
            else
            {
                var relatedSkills = await _relatedSkillsConverter.GetRelatedSkills(new List<ContentItem> { context.ContentItem });
                if (relatedSkills.Any() || relatedSkills.Count() == 1)
                {
                    var relatedSkill = relatedSkills.First();

                    foreach (var item in matches)
                    {
                        var socSkillMatrixIds = item.RelatedSkills?.Split(',').ToList() ?? new List<string>();
                        var rank = socSkillMatrixIds.IndexOf(context.ContentItem.ContentItemId) + 1;
                        jobprofileData.Add(new SocSkillMatrixContentItem
                        {
                            Id = id,
                            Title = context.ContentItem.Content.TitlePart.Title,
                            JobProfileId = Guid.Parse(item.GraphSyncPartId ?? string.Empty),
                            JobProfileTitle = item.JobProfileTitle,
                            Contextualised = relatedSkill.Contextualised ?? string.Empty,
                            ONetAttributeType = relatedSkill.ONetAttributeType ?? string.Empty,
                            Rank = rank,
                            ONetRank = relatedSkill.ONetRank ?? 0,
                            RelatedSOC = relatedSkill.RelatedSOC ?? new List<RelatedSocCodeItem>(),
                            RelatedSkill = relatedSkill.RelatedSkill?.FirstOrDefault()
                        });
                    }
                }
            }

            await _serviceBusMessageProcessor.SendOtherRelatedTypeMessages(jobprofileData, context.ContentItem.ContentType, actionType);
        }
    }
}
