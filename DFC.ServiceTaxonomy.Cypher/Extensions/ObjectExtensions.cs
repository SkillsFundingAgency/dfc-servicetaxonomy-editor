using System.Collections.Generic;
using System.Reflection;

namespace DFC.ServiceTaxonomy.Cypher.Extensions
{
    public static class ObjectExtensions
    {
        public static List<TModel> ToList<TDictionary, TModel>(this IList<object> source)
           where TDictionary : Dictionary<string, object>
           where TModel : class, new()
        {
            var result = new List<TModel>();

            foreach (var item in source)
            {
                var dict = item as TDictionary;

                if (dict != null)
                {
                    result.Add(dict.ToObject<TModel>());
                }
            }

            return result;
        }

        public static TModel ToObject<TModel>(this IDictionary<string, object> source)
            where TModel : class, new()
        {
            var result = new TModel();
            var resultType = result.GetType();

            foreach (var item in source)
            {
                resultType
                    .GetProperty(item.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                    ?.SetValue(result, item.Value, null);
            }

            return result;
        }
    }
}
