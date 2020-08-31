namespace DFC.ServiceTaxonomy.PageLocation.Models
{
    public class PageCloneResult
    {
        public PageCloneResult(string title, string urlName, string fullUrl)
        {
            Title = title;
            UrlName = urlName;
            FullUrl = fullUrl;
        }

        public string Title { get; }
        public string UrlName { get; }
        public string FullUrl { get; }
    }
}
