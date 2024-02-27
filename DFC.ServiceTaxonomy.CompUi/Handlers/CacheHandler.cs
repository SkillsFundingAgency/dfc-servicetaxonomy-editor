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
        var processing = _mapper.Map<Processing>(context);
        processing.EventType = ProcessingEvents.Published;

        await ProcessItem(processing);

        await base.PublishedAsync(context);
    }

    public async Task ProcessRemovedAsync(RemoveContentContext context)
    {
        var processing = _mapper.Map<Processing>(context);
        processing.EventType = ProcessingEvents.Removed;

        await ProcessItem(processing);

        await base.RemovedAsync(context);
    }

    public async Task ProcessUnpublishedAsync(PublishContentContext context)
    {
        var processing = _mapper.Map<Processing>(context);
        processing.EventType = ProcessingEvents.Unpublished;

        await ProcessItem(processing);

        await base.UnpublishedAsync(context);
    }

    public async Task ProcessDraftSavedAsync(SaveDraftContentContext context)
    {
        try
        {
            var processing = _mapper.Map<Processing>(context);
            processing.EventType = ProcessingEvents.DraftSaved;

            if (Enum.IsDefined(typeof(DraftContentTypes), processing.ContentType))
            {
                await _director.ProcessPageAsync(processing);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, message: $"Draft. Exception when invalidating draft.  " +
                $"Content Type: {context.ContentItem.ContentType}, " +
                $"Content item Id: {context.ContentItem.Id}.");
        }

        await base.DraftSavedAsync(context);
    }

    private async Task ProcessItem(Processing processing)
    {
        try
        {
            if (Enum.IsDefined(typeof(PublishedContentTypes), processing.ContentType))
            {
                switch (processing.ContentType)
                {
                    case nameof(PublishedContentTypes.SharedContent):
                        await _director.ProcessSharedContentAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.Page):
                        await _director.ProcessPageAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.Banner):
                        await _director.ProcessBannerAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.JobProfileCategory):
                        await _director.ProcessJobProfileCategoryAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.JobProfile):
                        await _director.ProcessJobProfileAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.Pagebanner):
                        await _director.ProcessPagebannerAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.TriageToolFilter):
                        await _director.ProcessTriageToolFilterAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.PersonalityFilteringQuestion):
                        await _director.ProcessPersonalityFilteringQuestionAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.PersonalityQuestionSet):
                        await _director.ProcessPersonalityQuestionSetAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.PersonalityShortQuestion):
                        await _director.ProcessPersonalityShortQuestionAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.PersonalityTrait):
                        await _director.ProcessPersonalityTraitAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.SOCSkillsMatrix):
                        await _director.ProcessSOCSkillsMatrixAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.WorkingPatterns):
                        await _director.ProcessWorkingPatternsAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.WorkingPatternDetail):
                        await _director.ProcessWorkingPatternDetailAsync(processing);
                        break;
                    case nameof(PublishedContentTypes.WorkingHoursDetail):
                        await _director.ProcessWorkingHoursDetailAsync(processing);
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
}


