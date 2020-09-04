using System;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class TitlePartCloneGenerator : ITitlePartCloneGenerator
    {
        public string Generate(string title)
        {
            if (title.StartsWith("CLONE - "))
            {
                return $"CLONE (2) - {title.Substring(8)}";
            }

            if (title.StartsWith("CLONE ("))
            {
                int index = Int32.Parse(title.Substring(7, 1));
                return $"CLONE ({++index}) - {title.Substring(title.IndexOf('-') + 2)}";
            }

            return $"CLONE - {title}";
        }
    }
}
