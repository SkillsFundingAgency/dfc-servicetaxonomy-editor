using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Extensions
{
    internal static class ListExtensions
    {
        public static string? ConvertListToCsv(this IList<string>? listOfvalues)
        {
            return listOfvalues == null ? null : string.Join(",", listOfvalues);
        }
    }
}
