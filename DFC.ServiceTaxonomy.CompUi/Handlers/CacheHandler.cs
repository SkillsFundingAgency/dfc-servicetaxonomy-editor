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
    private readonly IEventGridHandler _eventGridHandler;
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
        _eventGridHandler = eventGridHandler;
        _relatedContentItemIndexRepository = relatedContentItemIndexRepository;

        eventGridContentTypes = configuration.GetSection(eventGridAllowedContentTypeSettings).Get<List<string>>() ?? new List<string>();
        eventGridAllowedPages = configuration.GetSection(eventGridAllowedPageSettings).Get<List<string>>() ?? new List<string>();
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
        var processing = GetProcessingData(context, context.PreviousItem?.Content?.ToString(), ProcessingEvents.Published, FilterType.PUBLISHED);

        await base.PublishedAsync(context);

        await ProcessItem(processing);

        //Temp check to see if the published items content type matches any of the allowed items in the list
        var pageRouteFlag = PageRouteFlag(processing);

        if (pageRouteFlag && eventGridContentTypes.Any(contentType => context.ContentItem.ContentType.Contains(contentType, StringComparison.OrdinalIgnoreCase)))
        {
            var staxAction = context.PreviousItem == null ? ContentEventType.StaxCreate : ContentEventType.StaxUpdate;
            await ProcessEventGridMessage(processing, staxAction);
        }
    }

    public async Task ProcessRemovedAsync(RemoveContentContext context)
    {
        var processing = GetProcessingData(context, ProcessingEvents.Removed, FilterType.PUBLISHED);

        await base.RemovedAsync(context);

        await ProcessItem(processing);

        //Temp check to see if the published items content type matches any of the allowed items in the list
        var pageRouteFlag = PageRouteFlag(processing);

        if (eventGridContentTypes.Any(contentType => context.ContentItem.ContentType.Contains(contentType, StringComparison.OrdinalIgnoreCase)) && pageRouteFlag)
        {
            await ProcessEventGridMessage(processing, ContentEventType.StaxDelete);
        }
    }

    public async Task ProcessUnpublishedAsync(PublishContentContext context)
    {
        var processing = GetProcessingData(context, context.PreviousItem?.Content?.ToString() ?? context.ContentItem.Content.ToString(), ProcessingEvents.Unpublished, FilterType.PUBLISHED);

        await base.UnpublishedAsync(context);

        await ProcessItem(processing);

        //Temp check to see if the published items content type matches any of the allowed items in the list
        var pageRouteFlag = PageRouteFlag(processing);

        if (pageRouteFlag && eventGridContentTypes.Any(contentType => context.ContentItem.ContentType.Contains(contentType, StringComparison.OrdinalIgnoreCase)))
        {
            await ProcessEventGridMessage(processing, ContentEventType.StaxDelete);
        }
    }

    public async Task ProcessDraftSavedAsync(SaveDraftContentContext context)
    {
        var processing = GetProcessingData(context, ProcessingEvents.DraftSaved, FilterType.DRAFT);

        await base.DraftSavedAsync(context);

        await ProcessItem(processing);
    }

    public async Task ProcessEventGridMessage(Processing processing, ContentEventType contentEventType)
    {
        var current = JsonConvert.DeserializeObject<ContentItem>(processing.CurrentContent);

        if (current == null)
        {
            _logger.LogError($"Current content is null for following content type: {processing.ContentType}.");
            return;
        }

        switch (processing.ContentType)
        {
            case nameof(ContentTypes.Page) when contentEventType == ContentEventType.StaxUpdate:
                await ProcessGenericContentType(processing, current);
                break;
            case nameof(ContentTypes.SharedContent) when processing.EventType != ProcessingEvents.Created:
                await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                await ProcessSharedContent(processing);
                break;
            case nameof(ContentTypes.SectorLandingPage) when contentEventType == ContentEventType.StaxUpdate:
                await ProcessGenericContentType(processing, current);
                await ProcessJobProfilesLinkingtoSectorLandingPages(processing);
                break;
            case nameof(ContentTypes.JobProfileCategory) when contentEventType == ContentEventType.StaxUpdate:
                await ProcessGenericContentType(processing, current);
                await ProcessSharedContent(processing);
                break;
            case nameof(ContentTypes.Skill) when contentEventType == ContentEventType.StaxUpdate:
                await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                break;
            case nameof(ContentTypes.JobProfileSector) when contentEventType == ContentEventType.StaxUpdate:
                await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                await ProcessSharedContent(processing);
                break;
            case nameof(ContentTypes.JobProfile) when contentEventType == ContentEventType.StaxUpdate:
                await ProcessGenericContentType(processing, current);
                break;
            case nameof(ContentTypes.SOCSkillsMatrix) when contentEventType == ContentEventType.StaxUpdate:
                await ProcessSocSkillsMatrix(processing);
                break;
            case nameof(ContentTypes.HiddenAlternativeTitle):
            case nameof(ContentTypes.WorkingHoursDetail):
            case nameof(ContentTypes.WorkingPatternDetail):
            case nameof(ContentTypes.WorkingPatterns):
            case nameof(ContentTypes.JobProfileSpecialism):
            case nameof(ContentTypes.UniversityEntryRequirements):
            case nameof(ContentTypes.UniversityLink):
            case nameof(ContentTypes.UniversityRequirements):
            case nameof(ContentTypes.CollegeEntryRequirements):
            case nameof(ContentTypes.CollegeLink):
            case nameof(ContentTypes.CollegeRequirements):
            case nameof(ContentTypes.ApprenticeshipEntryRequirements):
            case nameof(ContentTypes.ApprenticeshipLink):
            case nameof(ContentTypes.ApprenticeshipRequirements):
            case nameof(ContentTypes.Restriction):
            case nameof(ContentTypes.DigitalSkills):
            case nameof(ContentTypes.Location):
            case nameof(ContentTypes.Environment):
            case nameof(ContentTypes.Uniform):
            case nameof(ContentTypes.SOCCode):
            case nameof(ContentTypes.Registration):
            case nameof(ContentTypes.DynamicTitlePrefix):
            case nameof(ContentTypes.RealStory):
                // Only want the message to be sent for related items to update an affected job profile.
                if (contentEventType == ContentEventType.StaxUpdate)
                {
                    await ProcessSharedContent(processing);
                }
                break;
            default:
                await _eventGridHandler.SendEventMessageAsync(TransformData(processing, current), contentEventType);
                break;
        };
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

    private async Task ProcessSharedContent(Processing processing)
    {
        var result = await _relatedContentItemIndexRepository.GetRelatedContentDataByContentItemIdAndPage(processing);

        //Note that the following limits sending messages for only Pages and JobProfiles.  This may be removed when working on DYSAC
        //as we'll need to send messages for PersonalityQuestionSet, PersonalityFilteringQuestion, PersonalityTrait & PersonalityShortQuestion etc.
        result?
            .Where(x => x.ContentType == nameof(ContentTypes.Page) || x.ContentType == nameof(ContentTypes.JobProfile))
            .ToList().ForEach(x => _eventGridHandler.SendEventMessageAsync(x, ContentEventType.StaxUpdate));
    }

    private async Task ProcessSocSkillsMatrix(Processing processing)
    {
        var result = await _relatedContentItemIndexRepository.GetRelatedContentDataByContentItemIdAndPage(processing);

        //Note that the following limits sending messages for only Pages and JobProfiles.  This may be removed when working on DYSAC
        //as we'll need to send messages for PersonalityQuestionSet, PersonalityFilteringQuestion, PersonalityTrait & PersonalityShortQuestion etc.
        result?
            .Where(x => x.ContentType == nameof(ContentTypes.JobProfile))
            .ToList().ForEach(x => _eventGridHandler.SendEventMessageAsync(x, ContentEventType.StaxUpdate));
    }

    private RelatedContentData TransformData(Processing processing, ContentItem contentItem)
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

    //Temp method to determine if page route matches what should currently be processed.
    //Can be removed once all routes are fine to be added.
    private bool PageRouteFlag(Processing processing)
    {
        //Temp check to filter pages by certain allowed url's/titles
        var current = JsonConvert.DeserializeObject<ContentItem>(processing.CurrentContent);
        var isAllowedPage = true;
        if (processing.ContentType == nameof(ContentTypes.Page) && !eventGridAllowedPages.Any(pageUrl => current.PageLocationParts.FullUrl.Contains(pageUrl)))
        {
            isAllowedPage = false;
        }

        return isAllowedPage;
    }
}
