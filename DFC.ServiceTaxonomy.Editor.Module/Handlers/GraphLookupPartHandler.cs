using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Parts;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.Editor.Module.Handlers
{
    public class GraphLookupPartHandler : ContentPartHandler<GraphLookupPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public GraphLookupPartHandler(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override Task UpdatedAsync(UpdateContentContext context, GraphLookupPart part)
        {
            //todo: what does this handler do?
            // var settings = GetSettings(part);
            //
            // if (!string.IsNullOrEmpty(settings.NodeLabel))
            // {
            //     part.Nodes = new (string id, string value)[0];
            //     part.ContentItem.DisplayText = "todo content item display text";
            //     part.Apply();
            // }

            part.Apply();

            return Task.CompletedTask;
        }

        private GraphLookupPartSettings GetSettings(GraphLookupPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, nameof(GraphLookupPart)));
            return contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();
        }
    }
}
