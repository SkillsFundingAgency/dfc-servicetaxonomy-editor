using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using DFC.ServiceTaxonomy.Editor.Module.Neo4j.Services;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using Neo4j.Driver;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    //todo: check button to show limit 10 examples? (similar to test connection)
    public class GraphLookupFieldDisplayDriver : ContentFieldDisplayDriver<GraphLookupField>
    {
        private readonly INeoGraphDatabase _neoGraphDatabase;

        public GraphLookupFieldDisplayDriver(INeoGraphDatabase neoGraphDatabase)
        {
            _neoGraphDatabase = neoGraphDatabase;
        }

        public override IDisplayResult Display(GraphLookupField field, BuildFieldDisplayContext fieldDisplayContext)
        {
            return Initialize<DisplayGraphLookupFieldViewModel>(GetDisplayShapeType(fieldDisplayContext), model =>
            {
                model.Field = field;
                model.Part = fieldDisplayContext.ContentPart;
                model.PartFieldDefinition = fieldDisplayContext.PartFieldDefinition;
            })
                //todo: location?
                .Location("Content")
                .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(GraphLookupField field, BuildFieldEditorContext context)
        {
            return Initialize<EditGraphLookupFieldViewModel>(GetEditorShapeType(context), model =>
            {
                // model.Url = context.IsNew ? settings.DefaultUrl : field.Url;

                model.ItemIds = field.Value;

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;

                model.SelectedItems = new List<VueMultiselectItemViewModel>();

                if (field.Value != null)
                {
                    model.SelectedItems.Add(new VueMultiselectItemViewModel
                    {
                        Value = field.Value, DisplayText = field.DisplayText
                    });
                }
            });
        }

        //todo: don't like new select appearing once selected disabled - nasty!
        // could just add trash can back to get functional first
        // then look to just allow the user to select using the same select
        //todo: add display names for node label and display field, then use in field label and hint
        public override async Task<IDisplayResult> UpdateAsync(GraphLookupField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var viewModel = new EditGraphLookupFieldViewModel();

            var modelUpdated = await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.ItemIds);

            if (modelUpdated)
            {
                field.Value = viewModel.ItemIds;

                //todo: here we get the display field from the graph, but it would be better to get it from the model
                var settings = context.PartFieldDefinition.GetSettings<GraphLookupFieldSettings>();

                var results = await _neoGraphDatabase.RunReadStatement(new Statement(
                        $"match (n:{settings.NodeLabel} {{{settings.ValueFieldName}:'{field.Value}'}}) return head(n.{settings.DisplayFieldName}) as displayField"),
                    r => r["displayField"].ToString());

                field.DisplayText = results.First();
            }

            return Edit(field, context);
        }
    }
}
