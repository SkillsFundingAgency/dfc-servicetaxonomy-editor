using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.JobProfilePartIndex.Extensions
{
    internal static class ListExtensions
    {
        public static string ConvertListToCsv(this IList<string> listOfvalues)
        {
            return string.Join(",", listOfvalues);
        }
    }
}
