using AutoMapper;
using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using DfE.NCS.Framework.Event.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.CompUi.Handlers;

public class CacheHandler : ContentHandlerBase, ICacheHandler
{
    private readonly ILogger<CacheHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IDirector _director;
    private readonly IBuilder _builder;
    private readonly IBackgroundQueue<Processing> _queue;
    private readonly IEventGridHandler _eventGridHandler;
    private readonly IConfiguration _configuration;
    private readonly IDataService _relatedContentItemIndexRepository;

    //Temp config to retrieve allowed event grid confiugrations from app settings for allowing certain content types and pages through
    private const string eventGridAllowedContentTypeSettings = "EventGridAllowedContentList";
    private const string eventGridAllowedPageSettings = "EventGridAllowedPagesList";
    private List<string> eventGridContentTypes;
    private List<string> eventGridAllowedPages;

    public CacheHandler(
        ILogger<CacheHandler> logger,
        IMapper mapper,
        IDirector director,
        IBuilder builder,
        IBackgroundQueue<Processing> queue,
        IEventGridHandler eventGridHandler,
        IConfiguration configuration,
        IDataService relatedContentItemIndexRepository
        )
    {
        _logger = logger;
        _mapper = mapper;
        _director = director;
        _builder = builder;
        _director.Builder = _builder;
        _queue = queue;
        _eventGridHandler = eventGridHandler;
        _configuration = configuration;
        _relatedContentItemIndexRepository = relatedContentItemIndexRepository;

        eventGridContentTypes = _configuration.GetSection(eventGridAllowedContentTypeSettings).Get<List<string>>() ?? new List<string>();
        eventGridAllowedPages = _configuration.GetSection(eventGridAllowedPageSettings).Get<List<string>>() ?? new List<string>();
    }

    public override async Task CreatedAsync(CreateContentContext context)
    {
        await ProcessCreatedAsync(context);
    }

    public override async Task PublishedAsync(PublishContentContext context)
    {
        await ProcessPublishedAsync(context);
    }

    public override async Task DraftSavedAsync(SaveDraftContentContext context)
    {
        await ProcessDraftSavedAsync(context);
    }

    public override async Task RemovedAsync(RemoveContentContext context)
    {
        await ProcessRemovedAsync(context);
    }

    public override async Task UnpublishedAsync(PublishContentContext context)
    {
        await ProcessUnpublishedAsync(context);
    }

    public async Task ProcessCreatedAsync(CreateContentContext context)
    {
        var processing = GetProcessingData(context, ProcessingEvents.Created, FilterType.PUBLISHED);

        await base.CreatedAsync(context);

        await SendEventGridMessage(processing);
    }

    public async Task ProcessPublishedAsync(PublishContentContext context)
    {
        var processing = GetProcessingData(context, context.PreviousItem?.Content?.ToString(), ProcessingEvents.Published, FilterType.PUBLISHED);

        await base.PublishedAsync(context);

        await ProcessItem(processing);

        await SendEventGridMessage(processing);

        if (processing.ContentType == ContentTypes.JobProfile.ToString())
        {
            await _queue.QueueItem(processing);
        }
    }

    public async Task ProcessRemovedAsync(RemoveContentContext context)
    {
        var processing = GetProcessingData(context, ProcessingEvents.Removed, FilterType.PUBLISHED);

        await base.RemovedAsync(context);

        await ProcessItem(processing);

        await SendEventGridMessage(processing);
    }

    public async Task ProcessUnpublishedAsync(PublishContentContext context)
    {
        var processing = GetProcessingData(context, context.PreviousItem?.Content?.ToString() ?? context.ContentItem.Content.ToString(), ProcessingEvents.Unpublished, FilterType.PUBLISHED);

        await base.UnpublishedAsync(context);

        await ProcessItem(processing);

        await SendEventGridMessage(processing);
    }

    public async Task ProcessDraftSavedAsync(SaveDraftContentContext context)
    {
        var processing = GetProcessingData(context, ProcessingEvents.DraftSaved, FilterType.DRAFT);

        await base.DraftSavedAsync(context);

        await ProcessItem(processing);

        if (processing.ContentType == ContentTypes.JobProfile.ToString())
        {
            await _queue.QueueItem(processing);
        }
    }

    public async Task SendEventGridMessage(Processing processing)
    {
        if (!eventGridContentTypes.Contains(processing.ContentType))
        {
            return;
        }

        switch (processing.ContentType)
        {
            case nameof(ContentTypes.Page):
                await ProcessPage(processing);
                break;
            case nameof(ContentTypes.SharedContent):
                await ProcessSharedContent(processing);
                break;
            default:
                _logger.LogError($"Content type could not be matched for Content Item Id: {processing.ContentItemId}");
                break;
        };
    }

    private async Task ProcessPage(Processing processing)
    {
        bool isFACPage = true;
        var current = JsonConvert.DeserializeObject<Models.ContentItem>(processing.CurrentContent);
        var previous = processing.PreviousContent != null ? JsonConvert.DeserializeObject<Models.ContentItem>(processing.PreviousContent) : new Models.ContentItem();
        var pageUrlChanged = current?.PageLocationParts?.FullUrl == previous?.PageLocationParts?.FullUrl ? false : true;

        if (processing.ContentType == nameof(ContentTypes.Page) && !eventGridAllowedPages.Any(pageUrl => (bool)(current?.PageLocationParts?.FullUrl.Contains(pageUrl))))
        {
            isFACPage = false;
        }

        if (isFACPage)
        {
            if (processing.EventType == ProcessingEvents.Created)
            {
                await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), ContentEventType.StaxCreate);
            }
            else if (processing.EventType == ProcessingEvents.Unpublished)
            {
                await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), ContentEventType.StaxDelete);
            }
            else
            {
                if (pageUrlChanged)
                {
                    await _eventGridHandler.SendEventMessageAsync(TransformData(processing, previous), ContentEventType.StaxDelete);
                    await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), ContentEventType.StaxCreate);
                }
                else
                {
                    await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), ContentEventType.StaxUpdate);
                }
            }
        }
    }

    private async Task ProcessSharedContent(Processing processing)
    {
        var current = JsonConvert.DeserializeObject<Models.ContentItem>(processing.CurrentContent);

        switch (processing.EventType)
        {
            case ProcessingEvents.Published:
                await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), ContentEventType.StaxUpdate);
                break;
            case ProcessingEvents.Removed:
                await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), ContentEventType.StaxDelete);
                break;
            case ProcessingEvents.Created:
                await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), ContentEventType.StaxCreate);
                break;
            case ProcessingEvents.Unpublished:
                await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), ContentEventType.StaxDelete);
                break;
        }

        if (processing.EventType != ProcessingEvents.Created)
        {
            var result = await _relatedContentItemIndexRepository.GetRelatedContentDataByContentItemIdAndPage(processing);

            if (result?.Count() > 0)
            {
                foreach (var item in result)
                {
                    if (item.ContentType == nameof(ContentTypes.Page))
                    {
                        await _eventGridHandler.SendEventMessageAsync(item, ContentEventType.StaxUpdate);
                    }
                }
            }
        }
    }

    private RelatedContentData TransformData(Processing processing, Models.ContentItem contentItem)
    {
        var contentData = _mapper.Map<RelatedContentData>(processing);
        contentData.FullPageUrl = contentItem?.PageLocationParts != null ? contentItem?.PageLocationParts?.FullUrl : string.Empty;
        contentData.GraphSyncId = contentItem?.GraphSyncParts?.Text != null ? contentItem?.GraphSyncParts?.Text : string.Empty;
        return contentData;
    }

    private async Task ProcessItem(Processing processing)
    {
        try
        {
            if (Enum.IsDefined(typeof(ContentTypes), processing.ContentType))
            {
                switch (processing.ContentType)
                {
                    case nameof(ContentTypes.SharedContent):
                        await _director.ProcessSharedContentAsync(processing);
                        break;
                    case nameof(ContentTypes.Page):
                        await _director.ProcessPageAsync(processing);
                        break;
                    case nameof(ContentTypes.Banner):
                        await _director.ProcessBannerAsync(processing);
                        break;
                    case nameof(ContentTypes.JobProfileCategory):
                        await _director.ProcessJobProfileCategoryAsync(processing);
                        break;
                    case nameof(ContentTypes.JobProfile):
                        await _director.ProcessJobProfileAsync(processing);
                        break;
                    case nameof(ContentTypes.Pagebanner):
                        await _director.ProcessPagebannerAsync(processing);
                        break;
                    case nameof(ContentTypes.TriageToolFilter):
                        await _director.ProcessTriageToolFilterAsync(processing);
                        break;
                    case nameof(ContentTypes.PersonalityFilteringQuestion):
                        await _director.ProcessPersonalityFilteringQuestionAsync(processing);
                        break;
                    case nameof(ContentTypes.PersonalityQuestionSet):
                        await _director.ProcessPersonalityQuestionSetAsync(processing);
                        break;
                    case nameof(ContentTypes.PersonalityShortQuestion):
                        await _director.ProcessPersonalityShortQuestionAsync(processing);
                        break;
                    case nameof(ContentTypes.PersonalityTrait):
                        await _director.ProcessPersonalityTraitAsync(processing);
                        break;
                    case nameof(ContentTypes.SOCSkillsMatrix):
                        await _director.ProcessSOCSkillsMatrixAsync(processing);
                        break;
                    case nameof(ContentTypes.WorkingPatterns):
                        await _director.ProcessWorkingPatternsAsync(processing);
                        break;
                    case nameof(ContentTypes.WorkingPatternDetail):
                        await _director.ProcessWorkingPatternDetailAsync(processing);
                        break;
                    case nameof(ContentTypes.WorkingHoursDetail):
                        await _director.ProcessWorkingHoursDetailAsync(processing);
                        break;
                    case nameof(ContentTypes.Skill):
                        await _director.ProcessSkillsAsync(processing);
                        break;
                    case nameof(ContentTypes.Taxonomy):
                        await _director.ProcessTaxonomyAsync(processing);
                        break;
                    default:
                        _logger.LogError($"ProcessItem. Content Item Id: {processing.DocumentId}, Content Type could not be determined: {processing.ContentType}, Event Type: {processing.EventType}");
                        break;
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"ProcessItem. Content Item Id: {processing.DocumentId}, Content Type could not be determined: {processing.ContentType}, Event Type: {processing.EventType}.");
        }
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
