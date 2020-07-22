using System;
using System.Collections.Generic;
using System.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class EnumerableExtensions
    {
        // tried morelinq's flatten, but could only get it to return leaf nodes
        // https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/Flatten.cs
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            return source.SelectMany(c => selector(c).Flatten(selector)).Concat(source);
        }
    }
}
