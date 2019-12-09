using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Configuration;
using DFC.ServiceTaxonomy.Editor.Module.Fields;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Drivers
{
    //todo: check button to show limit 10 examples? (similar to test connection)
    public class GraphLookupFieldDisplayDriver : ContentFieldDisplayDriver<GraphLookupField>
    {
        private readonly IOptionsMonitor<Neo4jConfiguration> _neo4JConfiguration;

        //todo: more appropriate to use IOptionsSnapshot?
        public GraphLookupFieldDisplayDriver(IOptionsMonitor<Neo4jConfiguration> neo4jConfiguration)
        {
            _neo4JConfiguration = neo4jConfiguration;
        }

        public override IDisplayResult Display(GraphLookupField field, BuildFieldDisplayContext fieldDisplayContext)
        {
            return Initialize<DisplayGraphLookupFieldViewModel>(GetDisplayShapeType(fieldDisplayContext), model =>
            {
                model.Field = field;
                model.Part = fieldDisplayContext.ContentPart;
                model.PartFieldDefinition = fieldDisplayContext.PartFieldDefinition;
            })
                .Location("Content")
                .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(GraphLookupField field, BuildFieldEditorContext context)
        {
            return Initialize<EditGraphLookupFieldViewModel>(GetEditorShapeType(context), model =>
            {
                // model.Url = context.IsNew ? settings.DefaultUrl : field.Url;
                model.DisplayText = "";
                model.Value = "";
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(GraphLookupField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.DisplayText, f => f.Value);

            return Edit(field, context);
        }
    }
}
