using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public class WhatYouWillDoMessageConverter : IMessageConverter<WhatYouWillDoData>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WhatYouWillDoMessageConverter> _logger;

        public WhatYouWillDoMessageConverter(IServiceProvider serviceProvider, ILogger<WhatYouWillDoMessageConverter> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public WhatYouWillDoData ConvertFrom(ContentItem contentItem)
        {
            try
            {
                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                List<ContentItem> relatedLocations = Helper.GetContentItems(contentItem.Content.JobProfile.Relatedlocations, contentManager);
                List<ContentItem> relatedEnvironments = Helper.GetContentItems(contentItem.Content.JobProfile.Relatedenvironments, contentManager);
                List<ContentItem> relatedUniforms = Helper.GetContentItems(contentItem.Content.JobProfile.Relateduniforms, contentManager);

                var whatYouWillDoData = new WhatYouWillDoData();
                whatYouWillDoData.DailyTasks = contentItem.Content.JobProfile.Daytodaytasks.Html;
                whatYouWillDoData.Locations = GetWYDRelatedItems(relatedLocations);
                whatYouWillDoData.Environments = GetWYDRelatedItems(relatedEnvironments);
                whatYouWillDoData.Uniforms = GetWYDRelatedItems(relatedUniforms);
                return whatYouWillDoData;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            
        }

        private IEnumerable<WYDRelatedContentType> GetWYDRelatedItems(List<ContentItem> contentItems)
        {
            var relatedContentTypes = new List<WYDRelatedContentType>();
            if (contentItems.Any())
            {
                foreach (var contentItem in contentItems)
                {
                    relatedContentTypes.Add(new WYDRelatedContentType
                    {
                        Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                        Url = contentItem.Content.GraphSyncPart.Text,
                        Description = GetWYDRelatedItemDescription(contentItem),
                        Title = contentItem.As<TitlePart>().Title,
                    });
                }
            }

            return Enumerable.Empty<WYDRelatedContentType>();
        }

        private string GetWYDRelatedItemDescription(ContentItem contentItem)
        {
            switch (contentItem.ContentType)
            {
                case ContentTypes.Location:
                    return contentItem.Content.Location.Description.Html;
                case ContentTypes.Environment:
                    return contentItem.Content.Environment.Description.Html;
                case ContentTypes.Uniform:
                    return contentItem.Content.Uniform.Description.Html;
                default: return string.Empty;
            }
        }


    }
}
