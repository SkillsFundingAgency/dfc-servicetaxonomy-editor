using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            //await updater.TryUpdateModelAsync(part, Prefix, t => t.Value, t => t.DisplayText);

            //return Edit(part);

            var viewModel = new GraphLookupPartViewModel();

            var modelUpdated = await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.ItemIds);

            if (modelUpdated)
            {
                part.Value = viewModel.ItemIds;

                //todo: here we get the display field from the graph, but it would be better to get it from the model
                //var settings = context.PartFieldDefinition.GetSettings<GraphLookupFieldSettings>();
                var settings = GetGraphLookupPartSettings(part);

                var results = await _neoGraphDatabase.RunReadStatement(new Statement(
                        $"match (n:{settings.NodeLabel} {{{settings.ValueFieldName}:'{part.Value}'}}) return head(n.{settings.DisplayFieldName}) as displayField"),
                    r => r["displayField"].ToString());

                part.DisplayText = results.First();
            }

            return Edit(part, context);
        }

        public GraphLookupPartSettings GetGraphLookupPartSettings(GraphLookupPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphLookupPart));
            return contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();
        }

        private void BuildViewModel(GraphLookupPartViewModel model, GraphLookupPart part, BuildPartEditorContext context)
        {
            var settings = GetGraphLookupPartSettings(part);

            model.Value = part.Value;
            model.DisplayText = part.DisplayText;

            model.ItemIds = part.Value;

            model.GraphLookupPart = part;
            //todo: view needs partname & field name, not whole PartFieldDefinition.
            //todo: check if really needs them, and if so, find them without this...
            model.PartFieldDefinition = context.TypePartDefinition.PartDefinition.Fields.First(f => f.Name == "todo");
            model.Settings = settings;

            model.SelectedItems = new List<VueMultiselectItemViewModel>();

            if (part.Value != null)
            {
                model.SelectedItems.Add(new VueMultiselectItemViewModel
                {
                    Value = part.Value,
                    DisplayText = part.DisplayText
                });
            }
        }
    }
}
