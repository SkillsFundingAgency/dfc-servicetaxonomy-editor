using System;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Title.Models;
using YesSql;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Handlers
{
    internal class JobProfileContentHandler : ContentHandlerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;
        private readonly ILogger<JobProfileContentHandler> _logger;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<JobProfileContentHandler> H;

        public JobProfileContentHandler(
            IServiceProvider serviceProvider,
            ISession session, ILogger<JobProfileContentHandler> logger, INotifier notifier, IHtmlLocalizer<JobProfileContentHandler> htmlLocalizer)
        {
            _session = session;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public override async Task DraftSavingAsync(SaveDraftContentContext context)
        {
            _logger.LogInformation($"DraftSavingAsync: context {context}");
            if (context.ContentItem.ContentType == ContentTypes.JobProfile)
            {
                // Only three job sectors can be selected
                var sectorContentItemIds = (JArray)context.ContentItem.Content.JobProfile.JobProfileSector.ContentItemIds;
                if (sectorContentItemIds.Count > 3)
                {
                    _logger.LogInformation("Only three job sectors can be selected.");
                    await _notifier.ErrorAsync(H["Only three job sectors can be selected."]);
                }
                else
                {
                    var socCode = await SkillReloadRequired(context);
                    if (!string.IsNullOrEmpty(socCode))
                    {
                        var socSkillsMatrixContentItemsList = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.SOCSkillsMatrix && c.DisplayText.StartsWith(socCode) && c.Published).ListAsync();
                        context.ContentItem.Content.JobProfile.Relatedskills.ContentItemIds = new JArray(socSkillsMatrixContentItemsList.Select(c => c.ContentItemId));
                    }
                }
            }
        }

        public override async Task PublishingAsync(PublishContentContext context)
        {
            if (context.ContentItem.ContentType == ContentTypes.JobProfile)
            {
                // Only three job sectors can be selected
                var sectorContentItemIds = (JArray)context.ContentItem.Content.JobProfile.JobProfileSector.ContentItemIds;
                if (sectorContentItemIds.Count > 3)
                {
                    context.Cancel = true;
                    _logger.LogInformation("Only three job sectors can be selected.");
                    await _notifier.ErrorAsync(H["Only three job sectors can be selected."]);
                }
                else
                {
                    var socCode = await SkillReloadRequired(context);
                    if (!string.IsNullOrEmpty(socCode))
                    {
                        var socSkillsMatrixContentItemsList = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.SOCSkillsMatrix && c.DisplayText.StartsWith(socCode) && c.Published).ListAsync();
                        if (context.ContentItem.Content.JobProfile.Relatedskills != null)
                        {
                            context.ContentItem.Content.JobProfile.Relatedskills.ContentItemIds = new JArray(socSkillsMatrixContentItemsList.Select(c => c.ContentItemId));
                        }
                    }

                }
            }
        }

        private async Task<string> SkillReloadRequired(ContentContextBase context)
        {
            var socCodeContentItemIds = (JArray)context.ContentItem.Content.JobProfile.SOCCode.ContentItemIds;
            var relatedSkillsIds = context.ContentItem.Content.JobProfile.Relatedskills == null ? default : (JArray)context.ContentItem.Content.JobProfile.Relatedskills.ContentItemIds;

            // If no SOC code assigned then no need to create the skill relationships
            if (socCodeContentItemIds == null || !socCodeContentItemIds.Any() || socCodeContentItemIds.Count != 1)
            {
                // Ensure any related skills are cleared
                if (relatedSkillsIds!.Any())
                {
                    context.ContentItem.Content.JobProfile.Relatedskills.ContentItemIds = null;
                }
                return string.Empty;
            }

            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            // If no related skills then go ahead and load related skills
            var socCodeContentItem = await contentManager.GetAsync(socCodeContentItemIds.First().Value<string>(), VersionOptions.Latest);
            var socCode = socCodeContentItem.As<TitlePart>().Title;
            if (relatedSkillsIds == null || !relatedSkillsIds.Any())
            {
                return socCode;
            }

            // Check skills relate to SOC code
            var relatedSkillsContentItems = await contentManager.GetAsync(relatedSkillsIds.Select(r => r.Value<string>()), true);
            // if no related to soc code then clear and reload
            if (!relatedSkillsContentItems.Any(c => c.DisplayText.StartsWith(socCode)))
            {
                context.ContentItem.Content.JobProfile.Relatedskills.ContentItemIds = null;
                return socCode;
            }
            // else we're all good
            return string.Empty;
        }
    }
}
