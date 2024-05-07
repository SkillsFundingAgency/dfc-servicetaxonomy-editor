using AutoMapper;
using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.CompUi.Handlers;

public class CacheHandler : ContentHandlerBase, ICacheHandler
{
    private readonly ILogger<CacheHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IDirector _director;
    private readonly IBuilder _builder;

    public CacheHandler(
        ILogger<CacheHandler> logger,
        IMapper mapper,
        IDirector director,
        IBuilder builder
        )
    {
        _logger = logger;
        _mapper = mapper;
        _director = director;
        _builder = builder;
        _director.Builder = _builder;
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
        var processing = GetProcessingData(context, ProcessingEvents.Published, FilterType.PUBLISHED);

        await ProcessItem(processing);

        await base.PublishedAsync(context);
    }

    public async Task ProcessRemovedAsync(RemoveContentContext context)
    {
        var processing = GetProcessingData(context, ProcessingEvents.Removed, FilterType.PUBLISHED);

        await ProcessItem(processing);

        await base.RemovedAsync(context);
    }

    public async Task ProcessUnpublishedAsync(PublishContentContext context)
    {
        var processing = GetProcessingData(context, ProcessingEvents.Unpublished, FilterType.PUBLISHED);

        await ProcessItem(processing);

        await base.UnpublishedAsync(context);
    }

    public async Task ProcessDraftSavedAsync(SaveDraftContentContext context)
    {
        var processing = GetProcessingData(context, ProcessingEvents.DraftSaved, FilterType.DRAFT);

        await ProcessItem(processing);

        await base.DraftSavedAsync(context);
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

    private Processing GetProcessingData(ContentContextBase context, ProcessingEvents processingEvent, FilterType filterType)
    {
        var processing = _mapper.Map<Processing>(context);
        processing.EventType = processingEvent;
        processing.FilterType = filterType.ToString();
        return processing;
    }
}


