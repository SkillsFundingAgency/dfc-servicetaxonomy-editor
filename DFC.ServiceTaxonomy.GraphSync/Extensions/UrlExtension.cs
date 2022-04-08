using System;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class UrlExtension
    {
        public static string ExtactCurieHref(this string href)
        {
            if(string.IsNullOrEmpty(href))
            {
                return string.Empty;
            }
            if(href.Contains("api/execute", StringComparison.InvariantCultureIgnoreCase))
            {
                return href.Substring(href.ToLower().IndexOf("api/execute") + 11);
            }
            return href;
        }
    }
}
