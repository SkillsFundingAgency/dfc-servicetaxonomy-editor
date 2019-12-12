using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Models;
using DFC.ServiceTaxonomy.Editor.Module.Neo4j.Services;
using DFC.ServiceTaxonomy.Editor.Module.Parts;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using Neo4j.Driver;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    //todo: gotta support multiple
    public class GraphLookupPartDisplayDriver : ContentPartDisplayDriver<GraphLookupPart>
    {
        private readonly INeoGraphDatabase _neoGraphDatabase;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public GraphLookupPartDisplayDriver(INeoGraphDatabase neoGraphDatabase, IContentDefinitionManager contentDefinitionManager)
        {
            _neoGraphDatabase = neoGraphDatabase;
            _contentDefinitionManager = contentDefinitionManager;
        }

        //todo: Display/DisplayAsync??

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

                    part.Nodes = await Task.WhenAll(ids.Select(async id => new GraphLookupNode(id, await GetNodeValue(id, settings))));
                }
            }

            return Edit(part, context);
        }

        public GraphLookupPartSettings GetGraphLookupPartSettings(GraphLookupPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphLookupPart));
            return contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();
        }

        private async Task<string> GetNodeValue(string id, GraphLookupPartSettings settings)
        {
            var results = await _neoGraphDatabase.RunReadStatement(new Statement(
                    $"match (n:{settings.NodeLabel} {{{settings.ValueFieldName}:'{id}'}}) return head(n.{settings.DisplayFieldName}) as displayField"),
                r => r["displayField"].ToString());

            return results.First();
        }

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

            //todo: use GraphLookupNode directly
            model.SelectedItems = new List<VueMultiselectItemViewModel>();

            foreach (var node in part.Nodes)
            {
                model.SelectedItems.Add(new VueMultiselectItemViewModel
                {
                    Value = node.Id,
                    DisplayText = node.DisplayText
                });
            }
        }
    }
}
