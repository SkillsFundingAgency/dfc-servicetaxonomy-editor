using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using DFC.ServiceTaxonomy.GraphLookup.Queries;
using DFC.ServiceTaxonomy.GraphLookup.ViewModels;
using DFC.ServiceTaxonomy.Neo4j.Services;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using GraphLookupPartSettings = DFC.ServiceTaxonomy.GraphLookup.Settings.GraphLookupPartSettings;

namespace DFC.ServiceTaxonomy.GraphLookup.Drivers
{
    public class GraphLookupPartDisplayDriver : ContentPartDisplayDriver<GraphLookupPart>
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public GraphLookupPartDisplayDriver(IGraphDatabase graphDatabase, IContentDefinitionManager contentDefinitionManager)
        {
            _graphDatabase = graphDatabase;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Edit(GraphLookupPart part, BuildPartEditorContext context)
        {
            return Initialize<GraphLookupPartViewModel>("GraphLookupPart_Edit", m => BuildViewModel(m, part, context));
        }

        public override async Task<IDisplayResult> UpdateAsync(GraphLookupPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new GraphLookupPartViewModel();

            bool modelUpdated = await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.ItemIds);
            if (modelUpdated)
            {
                if (viewModel.ItemIds == null)
                {
                    part.Nodes = new GraphLookupNode[0];
                }
                else
                {
                    string[] ids = viewModel.ItemIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    //todo: here we get the display field from the graph, but it would be better to get it from the model
                    // or if not only make 1 trip

                    var settings = GetGraphLookupPartSettings(part);

                    part.Nodes = await Task.WhenAll(ids.Select(async id => new GraphLookupNode(id, (await GetNodeValue(id, settings))!)));
                }
            }

            return Edit(part, context);
        }

        private GraphLookupPartSettings GetGraphLookupPartSettings(GraphLookupPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphLookupPart));
            return contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();
        }

        private async Task<string?> GetNodeValue(string id, GraphLookupPartSettings settings)
        {
            //todo: check if settings can be null
            var results = await _graphDatabase.RunReadQuery(new GetPropertyOnNodeQuery(
                settings.NodeLabel!,
                settings.ValueFieldName!,
                id,
                settings.DisplayFieldName!));

            return results.First();
        }

//todo: why does warning not appear on save, but later when view content items?
#pragma warning disable S1172 // Unused method parameters should be removed
        private void BuildViewModel(GraphLookupPartViewModel model, GraphLookupPart part, BuildPartEditorContext context)
#pragma warning restore S1172 // Unused method parameters should be removed
        {
            var settings = GetGraphLookupPartSettings(part);

            //todo: rename
            model.ItemIds = string.Join(",", part.Nodes.Select(n => n.Id));
            //todo: store id's also, so no need to go back to graph

            model.GraphLookupPart = part;
            //todo: view needs partname & field name, not whole PartFieldDefinition.
            //todo: check if really needs them, and if so, find them without this...
            //model.PartFieldDefinition = context.TypePartDefinition.PartDefinition.Fields.First(f => f.Name == "todo");
            model.PartName = context.TypePartDefinition.PartDefinition.Name;
            model.Settings = settings;

            model.SelectedItems = part.Nodes;
        }
    }
}
