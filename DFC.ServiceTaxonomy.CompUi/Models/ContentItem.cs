using System.Text.Json.Serialization;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class ContentItem
    {
        [JsonPropertyName("PageLocationPart")]
        public PageLocationParts? PageLocationParts { get; set; }

        [JsonPropertyName("GraphSyncPart")]
        public GraphSyncParts? GraphSyncParts { get; set; }

        [JsonPropertyName("TitlePart")]
        public TitlePart? TitlePart { get; set; }

        [JsonPropertyName("JobProfile")]
        public CurrentOpportunitiesData? JobProfile { get; set; }
    }

    public class GraphSyncParts
    {
        [JsonPropertyName("Text")]
        public string? Text { get; set; }
    }

    public class PageLocationParts
    {
        [JsonPropertyName("FullUrl")]
        public string? FullUrl { get; set; }

        [JsonPropertyName("RedirectLocations")]
        public string? RedirectLocations { get; set; }

        [JsonPropertyName("DefaultPageForLocation")]
        public bool? DefaultPageForLocation { get; set; }
    }

    public class TitlePart
    {
        [JsonPropertyName("Title")]
        public string? Title { get; set; }
    }

    public class CurrentOpportunitiesData
    {
        [JsonPropertyName("Coursekeywords")]
        public CourseKeywords? CourseKeywords { get; set; }

        [JsonPropertyName("SOCCode")]
        public ContentItemIds? SOCCode { get; set; }
    }

    public class CourseKeywords
    {
        [JsonPropertyName("Text")]
        public string? Text { get; set; }
    }
}
