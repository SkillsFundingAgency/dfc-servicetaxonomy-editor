using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Helpers
{
    public static class DocumentHelper
    {
        public static (string ContentType, Guid Id) GetContentTypeAndId(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return (string.Empty, Guid.Empty);
            }

            string pathOnly = uri.StartsWith("http") ? new Uri(uri, UriKind.Absolute).AbsolutePath : uri;
            pathOnly = pathOnly.ToLower().Replace("/api/execute", string.Empty);

            string[] uriParts = pathOnly.Trim('/').Split('/');
            string contentType = uriParts[0].ToLower();
            var id = Guid.Parse(uriParts[1]);

            return (contentType, id);
        }

        public static string GetAsString(object item)
        {
            if (item is Guid guidItem)
            {
                return guidItem.ToString();
            }

            if (item is JValue jValueItem)
            {
                return jValueItem.ToString(CultureInfo.InvariantCulture);
            }

            return (string)item;
        }

        public static string FirstCharToUpper(string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };

        public static Dictionary<string, object> SafeCastToDictionary(object? value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value is JObject valObj)
            {
                return valObj.ToObject<Dictionary<string, object>>()!;
            }

            if (!(value is Dictionary<string, object> dictionary))
            {
                throw new ArgumentException($"Didn't expect type {value.GetType().Name}");
            }

            return dictionary;
        }

        public static List<Dictionary<string, object>> SafeObjectArrayCastToDictionaryList(object? value)
        {
            var returnList = new List<Dictionary<string, object>>();
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value is JObject valObj)
            {
                returnList.Add(valObj.ToObject<Dictionary<string, object>>()!);
            }
            else if (value is JArray jArray)
            {
                returnList.AddRange(jArray.Select(jToken => ((JObject)jToken).ToObject<Dictionary<string, object>>()!));
            }
            else if (value is Dictionary<string, object> dictionary)
            {
                returnList.Add(dictionary);
            }
            else
            {
                throw new ArgumentException($"Didn't expect type {value.GetType().Name}");
            }

            return returnList;
        }

        public static List<Dictionary<string, object>> SafeCastToList(object? value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value is JArray valAry)
            {
                return valAry.ToObject<List<Dictionary<string, object>>>()!;
            }

            return (List<Dictionary<string, object>>)value;
        }
    }
}
