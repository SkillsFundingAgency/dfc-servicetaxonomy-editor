using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Dictionary<string, List<string>> GetLinks(this Dictionary<string, object> linksDictionary)
        {
            if (linksDictionary == null)
            {
                throw new ArgumentNullException(nameof(linksDictionary));
            }

            var result = new Dictionary<string, List<string>>();

            foreach (var item in linksDictionary.Where(link => link.Key != "self" && link.Key != "curies"))
            {
                var list = new List<string>();
                if(item.Value is JObject itemJObject)
                {
                    var dict = itemJObject.ToObject<Dictionary<string, object>>();
                    var uri = (string)dict!["href"];
                    if (!string.IsNullOrWhiteSpace(uri))
                    {
                        list.Add(uri);
                    }
                }
                else if (item.Value is JArray itemJArray)
                {
                    foreach (JToken arrayItem in itemJArray)
                    {
                        var dict = arrayItem.ToObject<Dictionary<string, object>>();
                        var uri = (string)dict!["href"];
                        if (!string.IsNullOrWhiteSpace(uri))
                        {
                            list.Add(uri);
                        }
                    }
                }
                result.Add(item.Key, list);
            }

            return result;
        }
    }
}
