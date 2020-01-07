using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphLookup.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.GraphLookup.Settings
{
    //todo: check button to show limit 10 examples? (similar to test connection)
    public class GraphLookupPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            if (!string.Equals(nameof(GraphLookupPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            return Initialize<GraphLookupPartSettingsViewModel>("GraphLookupPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<GraphLookupPartSettings>();

                model.DisplayName = settings.DisplayName;
                model.Hint = settings.Hint;

                model.Multiple = settings.Multiple;

                model.NodeLabel = settings.NodeLabel;
                model.DisplayFieldName = settings.DisplayFieldName;
                model.ValueFieldName = settings.ValueFieldName;
                model.RelationshipType = settings.RelationshipType;
                model.PropertyName = settings.PropertyName;
                model.NodesAreReadonly = settings.NodesAreReadonly;

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!string.Equals(nameof(GraphLookupPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            var model = new GraphLookupPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.DisplayName,
                m => m.Hint,
                m => m.Multiple,
                m => m.NodeLabel,
                m => m.DisplayFieldName,
                m => m.ValueFieldName,
                m => m.RelationshipType,
                m => m.PropertyName,
                m => m.NodesAreReadonly))
            {
                if (!string.IsNullOrEmpty(model.PropertyName) && model.Multiple)
                {
                    context.Updater.ModelState.AddModelError(nameof(model.PropertyName), "Setting a property name is only allowed if multiple nodes are not allowed.");
                }
                else
                {
                    context.Builder.WithSettings(new GraphLookupPartSettings
                    {
                        DisplayName = model.DisplayName,
                        Hint = model.Hint,
                        Multiple = model.Multiple,
                        NodeLabel = model.NodeLabel,
                        DisplayFieldName = model.DisplayFieldName,
                        ValueFieldName = model.ValueFieldName,
                        RelationshipType = model.RelationshipType,
                        PropertyName = model.PropertyName,
                        NodesAreReadonly = model.NodesAreReadonly
                    });
                }
            }

            return Edit(contentTypePartDefinition);
        }
    }
}
