using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.CustomEditor.Configuration
{
    public class RedirectUriConfiguration
    {
        public List<RedirectUriSettings>? PreviewRedirectUri { get; set; }

    }

    public class RedirectUriSettings
    {
        public string? RedirectUri { get; set; }
        public string? ContentType { get; set; }
    }
}
