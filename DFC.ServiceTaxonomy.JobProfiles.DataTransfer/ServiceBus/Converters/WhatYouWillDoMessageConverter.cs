﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Converters
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
                    DailyTasks = contentItem.Content.JobProfile.Daytodaytasks.Html,
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
                Url = string.Empty, //contentItem.Content.GraphSyncPart.Text,
                Description = GetWYDRelatedItemDescription(contentItem),
                Title = contentItem.As<TitlePart>().Title,
            }) ?? Enumerable.Empty<WYDRelatedContentType>();

        private static string GetWYDRelatedItemDescription(ContentItem contentItem) =>
            contentItem.ContentType switch
            {
                ContentTypes.Location => contentItem.Content.Location.Description.Text,
                ContentTypes.Environment => contentItem.Content.Environment.Description.Text,
                ContentTypes.Uniform => contentItem.Content.Uniform.Description.Text,
                _ => string.Empty,
            };
    }
}
