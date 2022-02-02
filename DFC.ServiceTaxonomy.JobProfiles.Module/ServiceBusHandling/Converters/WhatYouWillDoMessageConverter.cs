using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public class WhatYouWillDoMessageConverter : IMessageConverter<WhatYouWillDoData>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WhatYouWillDoMessageConverter> _logger;

        public WhatYouWillDoMessageConverter(ILogger<WhatYouWillDoMessageConverter> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<WhatYouWillDoData> ConvertFromAsync(ContentItem contentItem)
        {
            try
            {
                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                IEnumerable<ContentItem> relatedLocations = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RelatedLocations, contentManager);
                IEnumerable<ContentItem> relatedEnvironments = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RelatedEnvironments, contentManager);
                IEnumerable<ContentItem> relatedUniforms = await Helper.GetContentItemsAsync(contentItem.Content.JobProfile.RelatedUniforms, contentManager);

                var whatYouWillDoData = new WhatYouWillDoData
                {
                    DailyTasks = Helper.SanitiseHtmlWithPTag(contentItem.Content.JobProfile.Daytodaytasks.Html),
                    Locations = GetWYDRelatedItems(relatedLocations),
                    Environments = GetWYDRelatedItems(relatedEnvironments),
                    Uniforms = GetWYDRelatedItems(relatedUniforms)
                };
                return whatYouWillDoData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        private static IEnumerable<WYDRelatedContentType> GetWYDRelatedItems(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new WYDRelatedContentType
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Url = contentItem.Content.GraphSyncPart.Text,
                Description = GetWYDRelatedItemDescription(contentItem),
                Title = contentItem.As<TitlePart>().Title,
            }) ?? Enumerable.Empty<WYDRelatedContentType>();

        private static string GetWYDRelatedItemDescription(ContentItem contentItem) =>
            contentItem.ContentType switch
            {
                ContentTypes.Location => Helper.SanitiseHtml(contentItem.Content.Location.Description.Html),
                ContentTypes.Environment => Helper.SanitiseHtml(contentItem.Content.Environment.Description.Html),
                ContentTypes.Uniform => Helper.SanitiseHtml(contentItem.Content.Uniform.Description.Html),
                _ => string.Empty,
            };
    }
}
