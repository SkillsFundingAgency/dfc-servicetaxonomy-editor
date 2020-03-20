using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            if (!string.Equals(nameof(GraphSyncPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            return Initialize<GraphSyncPartSettingsViewModel>("GraphSyncPartSettings_Edit", model =>
                {
                    GraphSyncPartSettings graphSyncPartSettings = contentTypePartDefinition.GetSettings<GraphSyncPartSettings>();

                    model.BagPartContentItemRelationshipType = graphSyncPartSettings.BagPartContentItemRelationshipType;
                    model.PreexistingNode = graphSyncPartSettings.PreexistingNode;
                    model.NodeNameTransform = graphSyncPartSettings.NodeNameTransform;
                    model.PropertyNameTransform = graphSyncPartSettings.PropertyNameTransform;
                    model.CreateRelationshipType = graphSyncPartSettings.CreateRelationshipType;
                    model.IdPropertyName = graphSyncPartSettings.IdPropertyName;
                    model.GenerateIdPropertyValue = graphSyncPartSettings.GenerateIdPropertyValue;
                })
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!string.Equals(nameof(GraphSyncPart), contentTypePartDefinition.PartDefinition.Name))
            {
                return default!;
            }

            var model = new GraphSyncPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.BagPartContentItemRelationshipType,
                m => m.PreexistingNode,
                m => m.NodeNameTransform,
                m => m.PropertyNameTransform,
                m => m.CreateRelationshipType,
                m => m.IdPropertyName,
                m => m.GenerateIdPropertyValue))
            {
                context.Builder.WithSettings(new GraphSyncPartSettings
                {
                    BagPartContentItemRelationshipType = model.BagPartContentItemRelationshipType,
                    PreexistingNode = model.PreexistingNode,
                    NodeNameTransform = model.NodeNameTransform,
                    PropertyNameTransform = model.PropertyNameTransform,
                    CreateRelationshipType = model.CreateRelationshipType,
                    IdPropertyName = model.IdPropertyName,
                    GenerateIdPropertyValue = model.GenerateIdPropertyValue
                });
            }

            return Edit(contentTypePartDefinition);
        }
    }
}
