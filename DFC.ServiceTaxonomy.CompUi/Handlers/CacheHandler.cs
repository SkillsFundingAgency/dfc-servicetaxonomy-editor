using AutoMapper;
using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using DfE.NCS.Framework.Event.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Handlers;
using ContentItem = DFC.ServiceTaxonomy.CompUi.Models.ContentItem;

namespace DFC.ServiceTaxonomy.CompUi.Handlers;

public class CacheHandler : ContentHandlerBase, ICacheHandler
{
    private readonly ILogger<CacheHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IEventGridHandler _eventGridHandler;
    private readonly IDataService _relatedContentItemIndexRepository;

    private const string MetadataPage = "page-metadata";
    private const string MetadataJobProfile = "jobprofile-metadata";
    private const string MetadataJobProfileCategory = "jobprofilecategory-metadata";

    public CacheHandler(
        ILogger<CacheHandler> logger,
        IMapper mapper,
        IEventGridHandler eventGridHandler,
        IConfiguration configuration,
        IDataService relatedContentItemIndexRepository
        )
    {
        _logger = logger;
        _mapper = mapper;
        _eventGridHandler = eventGridHandler;
        _relatedContentItemIndexRepository = relatedContentItemIndexRepository;
    }

    public override async Task PublishedAsync(PublishContentContext context)
    {
        await ProcessPublishedAsync(context);
    }

    public override async Task RemovedAsync(RemoveContentContext context)
    {
        await ProcessRemovedAsync(context);
    }

    public override async Task UnpublishedAsync(PublishContentContext context)
    {
        await ProcessUnpublishedAsync(context);
    }

    public async Task ProcessPublishedAsync(PublishContentContext context)
    {
        var processing = GetProcessingData(context, context.PreviousItem?.Content?.ToString(), ProcessingEvents.Published, FilterType.PUBLISHED);

        await base.PublishedAsync(context);

        var staxAction = context.PreviousItem == null ? ContentEventType.StaxCreate : ContentEventType.StaxUpdate;
        await ProcessEventGridMessage(processing, staxAction);
    }

    public async Task ProcessRemovedAsync(RemoveContentContext context)
    {
        var processing = GetProcessingData(context, ProcessingEvents.Removed, FilterType.PUBLISHED);

        await base.RemovedAsync(context);

        await ProcessEventGridMessage(processing, ContentEventType.StaxDelete);
    }

    public async Task ProcessUnpublishedAsync(PublishContentContext context)
    {
        var processing = GetProcessingData(context, context.PreviousItem?.Content?.ToString() ?? context.ContentItem.Content.ToString(), ProcessingEvents.Unpublished, FilterType.PUBLISHED);

        await base.UnpublishedAsync(context);

        await ProcessEventGridMessage(processing, ContentEventType.StaxDelete);
    }

    public async Task ProcessEventGridMessage(Processing processing, ContentEventType contentEventType)
    {
        if (Enum.IsDefined(typeof(ContentTypes), processing.ContentType))
        {
            var current = JsonConvert.DeserializeObject<ContentItem>(processing.CurrentContent);

            if (current == null)
            {
                _logger.LogError($"Current content is null for following content type: {processing.ContentType}.");
                return;
            }

            switch (processing.ContentType)
            {
                case nameof(ContentTypes.Page): //when contentEventType == ContentEventType.StaxUpdate:
                    await SendMetadataOnlyMessage(MetadataPage, contentEventType);

                    if (contentEventType == ContentEventType.StaxUpdate)
                    {
                        await ProcessGenericContentType(processing, current);
                    }
                    break;
                case nameof(ContentTypes.SharedContent) when processing.EventType != ProcessingEvents.Created:
                    await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                    await ProcessRelatedContent(processing);
                    break;
                case nameof(ContentTypes.SectorLandingPage) when contentEventType == ContentEventType.StaxUpdate:
                    await ProcessGenericContentType(processing, current);
                    await ProcessJobProfilesLinkingtoSectorLandingPages(processing);
                    break;
                case nameof(ContentTypes.JobProfileCategory): //when contentEventType == ContentEventType.StaxUpdate:
                    await SendMetadataOnlyMessage(MetadataJobProfileCategory, contentEventType);

                    if (contentEventType == ContentEventType.StaxUpdate)
                    {
                        await ProcessGenericContentType(processing, current);
                        await ProcessRelatedContent(processing);
                    }
                    break;
                case nameof(ContentTypes.Skill) when contentEventType == ContentEventType.StaxUpdate:
                    await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                    break;
                case nameof(ContentTypes.JobProfileSector) when contentEventType == ContentEventType.StaxUpdate:
                    await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                    await ProcessRelatedContent(processing);
                    break;
                case nameof(ContentTypes.JobProfile):  //when contentEventType == ContentEventType.StaxUpdate:
                    await SendMetadataOnlyMessage(MetadataJobProfile, contentEventType);

                    if (contentEventType == ContentEventType.StaxUpdate)
                    {
                        await ProcessGenericContentType(processing, current);
                    }
                    break;
                case nameof(ContentTypes.ApprenticeshipEntryRequirements):
                case nameof(ContentTypes.ApprenticeshipLink):
                case nameof(ContentTypes.ApprenticeshipRequirements):
                case nameof(ContentTypes.CollegeEntryRequirements):
                case nameof(ContentTypes.CollegeLink):
                case nameof(ContentTypes.CollegeRequirements):
                case nameof(ContentTypes.DigitalSkills):
                case nameof(ContentTypes.DynamicTitlePrefix):
                case nameof(ContentTypes.Environment):
                case nameof(ContentTypes.HiddenAlternativeTitle):
                case nameof(ContentTypes.JobProfileSpecialism):
                case nameof(ContentTypes.Location):
                case nameof(ContentTypes.RealStory):
                case nameof(ContentTypes.Registration):
                case nameof(ContentTypes.Restriction):
                case nameof(ContentTypes.SOCCode):
                case nameof(ContentTypes.Uniform):
                case nameof(ContentTypes.UniversityEntryRequirements):
                case nameof(ContentTypes.UniversityLink):
                case nameof(ContentTypes.UniversityRequirements):
                case nameof(ContentTypes.WorkingHoursDetail):
                case nameof(ContentTypes.WorkingPatternDetail):
                case nameof(ContentTypes.WorkingPatterns):
                    // Only want the message to be sent for related items to update an affected job profile.
                    if (contentEventType == ContentEventType.StaxUpdate)
                    {
                        await ProcessRelatedContent(processing);
                    }
                    break;
                case nameof(ContentTypes.PersonalityQuestionSet):
                case nameof(ContentTypes.PersonalityFilteringQuestion):
                    await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                    break;
                case nameof(ContentTypes.PersonalityShortQuestion):
                    await ProcessRelatedContent(processing);
                    break;
                case nameof(ContentTypes.PersonalityTrait):
                case nameof(ContentTypes.SOCSkillsMatrix):
                    await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                    await ProcessRelatedContent(processing);
                    break;


                //To be merged into the above list when complete
                case nameof(ContentTypes.ApplicationView):
                case nameof(ContentTypes.FilterAdviceGroup):
                case nameof(ContentTypes.TriageLevelOne):
                case nameof(ContentTypes.TriageLevelTwo):
                case nameof(ContentTypes.TriageResultTile):
                case nameof(ContentTypes.TriageToolFilter):
                    await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                    //await ProcessRelatedContent(processing);
                    break;
                default:
                    await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                    break;
            };
        }
    }

    private async Task ProcessGenericContentType(Processing processing, ContentItem currentContent)
    {
        var previous = processing.PreviousContent != null ? JsonConvert.DeserializeObject<ContentItem>(processing.PreviousContent) : new ContentItem();
        var pageUrlChanged = currentContent?.PageLocationParts?.FullUrl == previous?.PageLocationParts?.FullUrl ? false : true;

        if (pageUrlChanged)
        {
            await _eventGridHandler.SendEventMessageAsync(TransformData(processing, previous), ContentEventType.StaxDelete);
            await _eventGridHandler.SendEventMessageAsync(TransformData(processing, currentContent), ContentEventType.StaxCreate);
        }
        else
        {
            await _eventGridHandler.SendEventMessageAsync(TransformData(processing, currentContent), ContentEventType.StaxUpdate);
        }
    }

    private async Task ProcessJobProfilesLinkingtoSectorLandingPages(Processing processing)
    {
        var result = await _relatedContentItemIndexRepository.GetRelatedContentDataByContentItemIdAndPage(processing);

        result?
            .Where(x => x.ContentType == nameof(ContentTypes.JobProfile))
            .ToList().ForEach(x => _eventGridHandler.SendEventMessageAsync(x, ContentEventType.StaxUpdate));
    }

    /// <summary>
    /// This method gets the related content items from RelatedContentItemIndex table the current item and then updates the related items where required too. 
    /// </summary>
    /// <param name="processing">The Processing object.</param>
    /// <returns>A Task is returned</returns>
    private async Task ProcessRelatedContent(Processing processing)
    {
        var result = await _relatedContentItemIndexRepository.GetRelatedContentDataByContentItemIdAndPage(processing);

        if (result.Count() > 0)
        {
            SendDistinctEventGridMessage(result);
            SendMultipleEventGridMessages(result);
        }
    }

    private void SendDistinctEventGridMessage(IEnumerable<RelatedContentData> dataList)
    {
        //Here we are placing a restriction of sending only one Event Grid message for content types listed below, as it doesn't make sense to send serveral duplicate messages relating to the same content.
        var contentTypes = new List<string> {
            nameof(ContentTypes.PersonalityShortQuestion),
            nameof(ContentTypes.PersonalityFilteringQuestion),
            nameof(ContentTypes.PersonalityTrait),
            nameof(ContentTypes.ApprenticeshipLink),
        };

        if (dataList.ToList().Any(x => contentTypes.Any(y => y == x.ContentType)))
        {
            dataList?
            .Where(x => contentTypes.Contains(x.ContentType))
            .DistinctBy(x => x.ContentType)
            .ToList().ForEach(x => _eventGridHandler.SendEventMessageAsync(x, ContentEventType.StaxUpdate));
        }
    }

    private void SendMultipleEventGridMessages(IEnumerable<RelatedContentData> dataList)
    {
        //Create a list of content types that we want to send Event Grid messages for.  Messages will not be sent for any other content types not in the list. 
        var contentTypes = new List<string> {
            nameof(ContentTypes.Page),
            nameof(ContentTypes.JobProfile),
            nameof(ContentTypes.PersonalityQuestionSet),
        };

        dataList?
        .Where(x => contentTypes.Contains(x.ContentType))
        .ToList().ForEach(x => _eventGridHandler.SendEventMessageAsync(x, ContentEventType.StaxUpdate));
    }

    private async Task SendMetadataOnlyMessage(string metadata, ContentEventType eventType)
    {
        var messageData = new RelatedContentData()
        {
            DisplayText = string.Empty,
            Author = "STAX automated message",
            ContentItemId = string.Empty,
            ContentType = metadata,
            FullPageUrl = string.Empty,
            GraphSyncId = string.Empty,
        };

        await _eventGridHandler.SendEventMessageAsync(messageData, eventType);
    }

    private RelatedContentData TransformData(Processing processing, ContentItem contentItem)
    {
        var contentData = _mapper.Map<RelatedContentData>(processing);
        contentData.FullPageUrl = (contentItem?.PageLocationParts?.FullUrl) ?? string.Empty;
        contentData.GraphSyncId = (contentItem?.GraphSyncParts?.Text) ?? string.Empty;
        return contentData;
    }

    private Processing GetProcessingData(ContentContextBase currentContext, ProcessingEvents processingEvent, FilterType filterType)
    {
        return GetProcessingData(currentContext, string.Empty, processingEvent, filterType);
    }

    private Processing GetProcessingData(ContentContextBase currentContext, string previousContent, ProcessingEvents processingEvent, FilterType filterType)
    {
        var processing = _mapper.Map<Processing>(currentContext);
        processing.PreviousContent = previousContent;
        processing.EventType = processingEvent;
        processing.FilterType = filterType.ToString();
        processing.FullUrl = FullUrl(processing.CurrentContent, processing.PreviousContent, processing.ContentType);

        return processing;
    }

    private string? FullUrl(string currentContent, string previousContent, string contentType)
    {
        if (contentType == ContentTypes.JobProfile.ToString())
        {
            var current = JsonConvert.DeserializeObject<Models.ContentItem>(currentContent);
            var previous = JsonConvert.DeserializeObject<Models.ContentItem>(previousContent);

            if (previous?.PageLocationParts != null || current.PageLocationParts != null)
            {
                if (string.IsNullOrWhiteSpace(previous?.PageLocationParts.FullUrl))
                {
                    return current.PageLocationParts.FullUrl;
                }
                else if (current.PageLocationParts.FullUrl.Equals(previous?.PageLocationParts.FullUrl))
                {
                    return current.PageLocationParts.FullUrl;
                }

                return previous.PageLocationParts.FullUrl;
            }
        }

        return null;
    }
}
