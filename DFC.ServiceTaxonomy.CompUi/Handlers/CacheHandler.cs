using AutoMapper;
using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
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
    private readonly IJobProfileCacheRefresh _jobProfileCacheRefresh;
    private bool test = false;

    public CacheHandler(
        ILogger<CacheHandler> logger,
        IMapper mapper,
        IDirector director,
        IBuilder builder,
        IBackgroundQueue<Processing> queue,
        IJobProfileCacheRefresh jobProfileCacheRefresh
        )
    {
        _logger = logger;
        _mapper = mapper;
        _director = director;
        _builder = builder;
        _director.Builder = _builder;
        _queue = queue;
        _jobProfileCacheRefresh = jobProfileCacheRefresh;
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

    public async Task ProcessPublishedAsync(PublishContentContext context)
    {
        var processing = GetProcessingData(context, context.PreviousItem?.Content?.ToString() ?? context.ContentItem.Content.ToString(), ProcessingEvents.Published, FilterType.PUBLISHED);

        await base.PublishedAsync(context);

        await ProcessItem(processing);

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
    }

    public async Task ProcessUnpublishedAsync(PublishContentContext context)
    {
        var processing = GetProcessingData(context, context.PreviousItem?.Content?.ToString() ?? context.ContentItem.Content.ToString(), ProcessingEvents.Unpublished, FilterType.PUBLISHED);

        await base.UnpublishedAsync(context);

        await ProcessItem(processing);
    }

    public async Task ProcessDraftSavedAsync(SaveDraftContentContext context)
    {
        var processing = GetProcessingData(context, ProcessingEvents.DraftSaved, FilterType.DRAFT);

        await base.DraftSavedAsync(context);

        await ProcessItem(processing);
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
                        //await _jobProfileCacheRefresh.RefreshAllJobProfileContent(processing);
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
                    default:
                        _logger.LogError($"ProcessItem. Content Item Id: {processing.DocumentId}, Content Type could not be determined: {processing.ContentType}, Event Type: {processing.EventType}");
                        break;
                }
                /*if (processing.ContentType == ContentTypes.JobProfile.ToString())
                {
                    await _jobProfileCacheRefresh.RefreshAllJobProfileContent(processing);
                }*/
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

    private string? FullUrl(string currentContent, string previousContent, string contentType )
    {
        if (contentType == ContentTypes.JobProfile.ToString())
        {
            var current = JsonConvert.DeserializeObject<Page>(currentContent);
            var previous = JsonConvert.DeserializeObject<Page>(previousContent);

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
