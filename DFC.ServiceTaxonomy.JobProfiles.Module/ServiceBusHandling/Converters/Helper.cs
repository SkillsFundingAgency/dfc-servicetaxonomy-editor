﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Extensions;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;

using Newtonsoft.Json.Linq;

using OrchardCore.ContentManagement;
using OrchardCore.Title.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters
{
    public static class Helper
    {
        public static async Task<IEnumerable<ContentItem>> GetContentItemsAsync(dynamic contentPicker, IContentManager contentManager)
        {
            if (contentPicker != null)
            {
                var contentItemIds = (JArray)contentPicker.ContentItemIds;
                if (contentItemIds.Any())
                {
                    var idList = contentItemIds.Select(c => c.Value<string>());
                    var contentItems = await contentManager.GetAsync(idList);
                    return contentItems;
                }
            }

            return Enumerable.Empty<ContentItem>();
        }

        public static async Task<IEnumerable<string>> GetContentItemNamesAsync(dynamic contentPicker, IContentManager contentManager)
        {
            IList<string> contentItemNames = new List<string>();
            if (contentPicker != null)
            {
                var contentItemIds = (JArray)contentPicker.ContentItemIds;
                if (contentItemIds.Any())
                {
                    var idList = contentItemIds.Select(c => c.Value<string>());
                    var contentItems = await contentManager.GetAsync(idList);
                    foreach (var item in contentItems)
                    {
                        contentItemNames.Add(item.ContentItem.DisplayText);
                    }
                    return contentItemNames;
                }
            }

            return Enumerable.Empty<string>();
        }

        public static async Task<IEnumerable<string>> GetRelatedSkillsAsync(dynamic contentPicker, IContentManager contentManager)
        {
            IList<string> contentItemNames = new List<string>();
            if (contentPicker != null)
            {
                var contentItemIds = (JArray)contentPicker.ContentItemIds;
                if (contentItemIds.Any())
                {
                    var idList = contentItemIds.Select(c => c.Value<string>());
                    var contentItems = await contentManager.GetAsync(idList);
                    foreach (var item in contentItems)
                    {
                        contentItemNames.Add(GetSlugValue(item.ContentItem.Content.SOCSkillsMatrix.RelatedSkill.Text.ToString()));
                    }
                    return contentItemNames;
                }
            }

            return Enumerable.Empty<string>();
        }

        public static string GetSlugValue(string field)
        {
            string UrlNameRegexPattern = @"[^\w\-\!\$\'\(\)\=\@\d_]+";
            return string.IsNullOrWhiteSpace(field) ? string.Empty : Regex.Replace(field.ToLower().Trim(), UrlNameRegexPattern, "-");

        }

        public static string GetHyphenated(string field)
        {
            return field.Replace(" ", "-");
        }

        public static IEnumerable<Classification> MapClassificationData(IEnumerable<ContentItem> contentItems) =>
            contentItems?.Select(contentItem => new Classification
            {
                Id = contentItem.As<GraphSyncPart>().ExtractGuid(),
                Title = contentItem.As<TitlePart>().Title,
                Description = GetClassificationDescriptionText(contentItem),
                Url = GetClassificationURLText(contentItem)
            }) ?? Enumerable.Empty<Classification>();

        private static string GetClassificationDescriptionText(ContentItem contentItem) =>
            contentItem.ContentType switch
            {
                ContentTypes.HiddenAlternativeTitle => contentItem.Content.HiddenAlternativeTitle.Description.Text,
                ContentTypes.JobProfileSpecialism => contentItem.Content.JobProfileSpecialism.Description.Text,
                ContentTypes.WorkingHoursDetail => contentItem.Content.WorkingHoursDetail.Description.Text,
                ContentTypes.WorkingPatterns => contentItem.Content.WorkingPatterns.Description.Text,
                ContentTypes.WorkingPatternDetail => contentItem.Content.WorkingPatternDetail.Description.Text,
                _ => string.Empty,
            };

        private static string GetClassificationURLText(ContentItem contentItem) =>
           contentItem.ContentType switch
           {
               ContentTypes.ApprenticeshipStandard => contentItem.Content.ApprenticeshipStandard.LARScode.Text,
               ContentTypes.WorkingPatternDetail => GetSlugValue(contentItem.As<TitlePart>().Title),
               ContentTypes.WorkingHoursDetail => GetSlugValue(contentItem.As<TitlePart>().Title),
               ContentTypes.WorkingPatterns => GetSlugValue(contentItem.As<TitlePart>().Title),
               _ => string.Empty,
           };

        public static string SanitiseHtml(dynamic html)
        {
            string htmlString = ((string)html).Replace("<br>", "");
            return Regex.Replace(htmlString, "<p[^>]*>|</p[^>]*>|</span[^>]*>|<span[^>]*>|^<ul><li>|</li></ul>$", "");
        }

        public static string SanitiseHtmlWithPTag(dynamic html)
        {
            string htmlString = ((string)html).Replace("<br>", "").Replace("&nbsp;", " ");
            return Regex.Replace(htmlString, "</span[^>]*>|<span[^>]*>|^<ul><li>|</li></ul>$", "");
        }
    }
}
