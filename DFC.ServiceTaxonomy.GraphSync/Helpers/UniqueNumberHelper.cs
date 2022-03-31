using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Helpers
{
    public static class UniqueNumberHelper
    {
        private static int s_number;
        private static readonly Dictionary<string, int> s_dictionary = new Dictionary<string, int>();

        public static int GetNumber(string id)
        {
            if (s_dictionary.ContainsKey(id))
            {
                return s_dictionary[id];
            }

            s_dictionary.Add(id, s_number);
            return s_number++;
        }
    }
}
