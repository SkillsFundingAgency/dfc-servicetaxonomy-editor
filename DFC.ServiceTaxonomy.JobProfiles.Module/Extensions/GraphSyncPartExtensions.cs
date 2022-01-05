using System;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Extensions
{
    public static class GraphSyncPartExtensions
    {
        public static Guid ExtractGuid(this GraphSyncPart part)
        {
            var text = part.Text?.Substring(part.Text.LastIndexOf('/') + 1) ?? String.Empty;
            if(!string.IsNullOrWhiteSpace(text))
            {
                return Guid.Parse(text);
            }
            return Guid.Empty;
        }
    }
}
