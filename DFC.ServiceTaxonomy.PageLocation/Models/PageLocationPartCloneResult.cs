namespace DFC.ServiceTaxonomy.PageLocation.Models
{
    public class PageLocationPartCloneResult
    {
        public PageLocationPartCloneResult(string urlName, string fullUrl)
        {
            UrlName = urlName;
            FullUrl = fullUrl;
        }

        public string UrlName { get; }
        public string FullUrl { get; }
    }
}
