using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.CompUi.Models
{
    public class ContentPageModel
    {
        [JsonProperty("PartitionKey")]
        public string? PartitionKey { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("Path")]
        public string Path { get; set; }

        [JsonProperty("TopNavigationText")]
        public object TopNavigationText { get; set; }

        [JsonProperty("TopNavigationOrder")]
        public int? TopNavigationOrder { get; set; }

        [JsonProperty("CdnLocation")]
        public object CdnLocation { get; set; }

        [JsonProperty("Layout")]
        public int? Layout { get; set; }

        [JsonProperty("IsOnline")]
        public bool? IsOnline { get; set; }

        [JsonProperty("OfflineHtml")]
        public string OfflineHtml { get; set; }

        [JsonProperty("PhaseBannerHtml")]
        public string PhaseBannerHtml { get; set; }

        [JsonProperty("SitemapURL")]
        public string SitemapURL { get; set; }

        [JsonProperty("ExternalURL")]
        public object ExternalURL { get; set; }

        [JsonProperty("RobotsURL")]
        public string RobotsURL { get; set; }

        [JsonProperty("DateOfRegistration")]
        public DateTime? DateOfRegistration { get; set; }

        [JsonProperty("LastModifiedDate")]
        public DateTime? LastModifiedDate { get; set; }

        [JsonProperty("Regions")]
        public List<Region> Regions { get; set; }

        [JsonProperty("AjaxRequests")]
        public object AjaxRequests { get; set; }

        [JsonProperty("PageLocations")]
        public Dictionary<string, PageLocations> PageLocations { get; set; }

        [JsonProperty("JavaScriptNames")]
        public JavaScriptNames JavaScriptNames { get; set; }

        [JsonProperty("CssScriptNames")]
        public object CssScriptNames { get; set; }

        [JsonProperty("IsInteractiveApp")]
        public bool? IsInteractiveApp { get; set; }

        [JsonProperty("TraceId")]
        public string TraceId { get; set; }

        [JsonProperty("ParentId")]
        public string ParentId { get; set; }

    }

    public class Region
    {
        [JsonProperty("PageRegion")]
        public int? PageRegion { get; set; }

        [JsonProperty("IsHealthy")]
        public bool? IsHealthy { get; set; }

        [JsonProperty("RegionEndpoint")]
        public string RegionEndpoint { get; set; }

        [JsonProperty("HealthCheckRequired")]
        public bool? HealthCheckRequired { get; set; }

        [JsonProperty("OfflineHtml")]
        public string OfflineHtml { get; set; }

        [JsonProperty("DateOfRegistration")]
        public DateTime? DateOfRegistration { get; set; }

        [JsonProperty("LastModifiedDate")]
        public DateTime? LastModifiedDate { get; set; }
    }

    public class JavaScriptNames
    {
        [JsonProperty("/js/dfc-app-pages.min.js")]
        public string JsDfcAppPagesMinJs { get; set; }
    }

    public class PageLocations
    {
        [JsonProperty("Locations")]
        public List<string?> Locations { get; set; }

    }
}
