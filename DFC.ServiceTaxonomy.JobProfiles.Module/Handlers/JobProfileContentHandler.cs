using System;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json.Linq;

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;

using YesSql;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Handlers
{
    internal class JobProfileContentHandler : ContentHandlerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;

        public JobProfileContentHandler(
            IServiceProvider serviceProvider,
            ISession session)
        {
            _session = session;
            _serviceProvider = serviceProvider;
        }
        public override async Task DraftSavingAsync(SaveDraftContentContext context)
        {
            if (context.ContentItem.ContentType == ContentTypes.JobProfile)
            {
                //
                var socCodeContentItemIds = (JArray)context.ContentItem.Content.JobProfile.SOCCode.ContentItemIds;

                if (socCodeContentItemIds.Any() && socCodeContentItemIds.Count == 1)
                {
                    var socCodeContentItemId = socCodeContentItemIds.First().Value<string>();
                    var contentManager = _serviceProvider.GetRequiredService<IContentManager>(); 
                    var socCodeContentItem = await contentManager.GetAsync(socCodeContentItemId, VersionOptions.Latest);
                    var socCode = (string)socCodeContentItem.Content.SOCCode.SOCCodeTextField.Text;
                    var socSkillsMatrixContentItemsList = await _session.Query<ContentItem, ContentItemIndex>(c => c.ContentType == ContentTypes.SOCSkillsMatrix && c.DisplayText.StartsWith(socCode)).ListAsync();
                    context.ContentItem.Content.JobProfile.Relatedskills.ContentItemIds = new JArray(socSkillsMatrixContentItemsList.Select(c => c.ContentItemId));
                }
            }
        }
    }
}
