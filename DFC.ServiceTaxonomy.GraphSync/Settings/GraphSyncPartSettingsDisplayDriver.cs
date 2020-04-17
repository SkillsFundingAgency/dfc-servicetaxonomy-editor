using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace DFC.ServiceTaxonomy.GraphSync.Settings
{
    public class GraphSyncPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly IOptionsMonitor<GraphSyncPartSettingsConfiguration> _graphSyncPartSettings;

        public GraphSyncPartSettingsDisplayDriver(IOptionsMonitor<GraphSyncPartSettingsConfiguration> graphSyncPartSettings)
        {
            _graphSyncPartSettings = graphSyncPartSettings;
        }

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

                    BuildGraphSyncPartSettingsList(model);
                })
                .Location("Content");
        }

        private void BuildGraphSyncPartSettingsList(GraphSyncPartSettingsViewModel model)
        {
            var listToReturn = new List<SelectListItem>();

            listToReturn.Add(new SelectListItem("Custom", "Custom"));

            foreach (var item in _graphSyncPartSettings.CurrentValue.Settings)
            {
                listToReturn.Add(new SelectListItem(item.Name, item.Name));

                //TODO: Move equals function elsewhere for Single Reponsibility.
                //todo: if model.X is null, but item.X isn't null, model.X!.Equals will throw - forgiving null when it is null
                if (((string.IsNullOrWhiteSpace(model.BagPartContentItemRelationshipType) && string.IsNullOrWhiteSpace(item.BagPartContentItemRelationshipType)) || model.BagPartContentItemRelationshipType!.Equals(item.BagPartContentItemRelationshipType, StringComparison.CurrentCultureIgnoreCase))
                    && ((string.IsNullOrWhiteSpace(model.NodeNameTransform) && string.IsNullOrWhiteSpace(item.NodeNameTransform)) || model.NodeNameTransform!.Equals(item.NodeNameTransform, StringComparison.CurrentCultureIgnoreCase))
                    && ((string.IsNullOrWhiteSpace(model.PropertyNameTransform) && string.IsNullOrWhiteSpace(item.PropertyNameTransform)) || model.PropertyNameTransform!.Equals(item.PropertyNameTransform, StringComparison.CurrentCultureIgnoreCase))
                    && ((string.IsNullOrWhiteSpace(model.CreateRelationshipType) && string.IsNullOrWhiteSpace(item.CreateRelationshipType)) || model?.CreateRelationshipType!.Equals(item.CreateRelationshipType, StringComparison.CurrentCultureIgnoreCase))
                    && ((string.IsNullOrWhiteSpace(model.IdPropertyName) && string.IsNullOrWhiteSpace(item.IdPropertyName)) || model.IdPropertyName!.Equals(item.IdPropertyName, StringComparison.CurrentCultureIgnoreCase))
                    && ((string.IsNullOrWhiteSpace(model.GenerateIdPropertyValue) && string.IsNullOrWhiteSpace(item.GenerateIdPropertyValue)) || model?.GenerateIdPropertyValue!.Equals(item.GenerateIdPropertyValue, StringComparison.CurrentCultureIgnoreCase)))
                {
                    model.SelectedSetting = item.Name;
                    model.ReadOnly = true;


                }
                model.AllSettings.Add(item);
            }

            model.Settings = listToReturn;
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
