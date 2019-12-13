using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Parts;
using DFC.ServiceTaxonomy.Editor.Module.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.Editor.Module.Settings
{
    //todo: check button to show limit 10 examples? (similar to test connection)
    public class GraphLookupPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
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
                model.NodesAreReadonly = settings.NodesAreReadonly;

                model.GraphLookupPartSettings = settings;
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
                m => m.NodesAreReadonly))
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
                    NodesAreReadonly = model.NodesAreReadonly
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
