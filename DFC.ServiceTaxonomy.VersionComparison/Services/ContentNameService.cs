using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.VersionComparison.Services
{
    public class ContentNameService : IContentNameService
    {
        private readonly IContentManager _contentManager;

        public ContentNameService(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task<string> GetContentNameAsync(string contentItemId)
        {
            if (string.IsNullOrWhiteSpace(contentItemId))
            {
                return string.Empty;
            }

            var contentItem = await _contentManager.GetAsync(contentItemId);
            if (contentItem != null)
            {
                return contentItem.DisplayText;
            }

            return string.Empty;
        }
    }
}
