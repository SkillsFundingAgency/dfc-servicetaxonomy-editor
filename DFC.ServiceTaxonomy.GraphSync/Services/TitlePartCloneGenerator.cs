using DFC.ServiceTaxonomy.GraphSync.Services.Interface;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class TitlePartCloneGenerator : ITitlePartCloneGenerator
    {
        public string Generate(string title)
        {
            return title.StartsWith("CLONE - ") ? title : $"CLONE - {title}";
        }
    }
}
