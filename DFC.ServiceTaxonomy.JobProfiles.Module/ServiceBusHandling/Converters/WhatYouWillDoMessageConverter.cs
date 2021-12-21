using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public class WhatYouWillDoMessageConverter : IMessageConverter<WhatYouWillDoData>
    {
        private readonly IServiceProvider _serviceProvider;

        public WhatYouWillDoMessageConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public WhatYouWillDoData ConvertFrom(ContentItem contentItem)
        {
            try
            {
                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                List<ContentItem> relatedLocations = GetContentItems(contentItem.Content.JobProfile.Relatedlocations, contentManager);
                List<ContentItem> relatedEnvironments = GetContentItems(contentItem.Content.JobProfile.Relatedenvironments, contentManager);
                List<ContentItem> relatedUniforms = GetContentItems(contentItem.Content.JobProfile.Relateduniforms, contentManager);

                var whatYouWillDoData = new WhatYouWillDoData();
                whatYouWillDoData.DailyTasks = contentItem.Content.JobProfile.Daytodaytasks.Html;
                whatYouWillDoData.Locations = GetWYDRelatedItems(relatedLocations);
                whatYouWillDoData.Environments = GetWYDRelatedItems(relatedEnvironments);
                whatYouWillDoData.Uniforms = GetWYDRelatedItems(relatedUniforms);
                return whatYouWillDoData;
            }
            catch(Exception ex)
            {
                // TODO : Add Error handling
                Console.WriteLine(ex.Message);
                throw;
            }
            
        }

        private List<ContentItem> GetContentItems(dynamic contentPicker, IContentManager contentManager)
        {
            var contentItemIds = (JArray)contentPicker.ContentItemIds;
            if (contentItemIds.Any())
            {
                var idList = contentItemIds.Select(c => c.Value<string>()).ToList();
                var contentItems = contentManager.GetAsync(idList).Result;
                return contentItems.ToList();
            }

            return new List<ContentItem>();
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
                case "Location":
                    return contentItem.Content.Location.Description.Html;
                case "Environment":
                    return contentItem.Content.Environment.Description.Html;
                case "Uniform":
                    return contentItem.Content.Uniform.Description.Html;
                default: return string.Empty;
            }
        }


    }
}
