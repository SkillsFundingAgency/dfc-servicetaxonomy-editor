using System;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class JObjectExtensions
    {
        public static DateTime? GetDateTime(this JObject jobject, string name)
        {
            object? val = jobject[name];
            return !string.IsNullOrEmpty(val?.ToString())
                ? DateTime.Parse(val.ToString()!)
                : (DateTime?) null;
        }
    }
}
