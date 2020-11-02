using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

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
                    model.DisplayId = graphSyncPartSettings.DisplayId;
                    model.NodeNameTransform = graphSyncPartSettings.NodeNameTransform;
                    model.PropertyNameTransform = graphSyncPartSettings.PropertyNameTransform;
                    model.CreateRelationshipType = graphSyncPartSettings.CreateRelationshipType;
                    model.IdPropertyName = graphSyncPartSettings.IdPropertyName;
                    model.GenerateIdPropertyValue = graphSyncPartSettings.GenerateIdPropertyValue;
                    model.PreExistingNodeUriPrefix = graphSyncPartSettings.PreExistingNodeUriPrefix;
                    model.VisualiserNodeDepth = graphSyncPartSettings.VisualiserNodeDepth;
                    model.VisualiserIncomingRelationshipsPathLength =
                        graphSyncPartSettings.VisualiserIncomingRelationshipsPathLength;

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
                if (IsEqual(model.BagPartContentItemRelationshipType, item.BagPartContentItemRelationshipType)
                    && IsEqual(model.NodeNameTransform, item.NodeNameTransform)
                    && IsEqual(model.PropertyNameTransform, item.PropertyNameTransform)
                    && IsEqual(model.CreateRelationshipType, item.CreateRelationshipType)
                    && IsEqual(model.IdPropertyName, item.IdPropertyName)
                    && IsEqual(model.GenerateIdPropertyValue, item.GenerateIdPropertyValue)
                    && IsEqual(model.PreExistingNodeUriPrefix, item.PreExistingNodeUriPrefix)
                    && model.PreexistingNode == item.PreexistingNode
                    && model.DisplayId == item.DisplayId)
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
                m => m.DisplayId,
                m => m.NodeNameTransform,
                m => m.PropertyNameTransform,
                m => m.CreateRelationshipType,
                m => m.IdPropertyName,
                m => m.GenerateIdPropertyValue,
                m => m.PreExistingNodeUriPrefix,
                m => m.VisualiserNodeDepth,
                m => m.VisualiserIncomingRelationshipsPathLength)
                && Valid(model, context))
            {
                context.Builder.WithSettings(new GraphSyncPartSettings
                {
                    BagPartContentItemRelationshipType = model.BagPartContentItemRelationshipType,
                    PreexistingNode = model.PreexistingNode,
                    DisplayId = model.DisplayId,
                    NodeNameTransform = model.NodeNameTransform,
                    PropertyNameTransform = model.PropertyNameTransform,
                    CreateRelationshipType = model.CreateRelationshipType,
                    IdPropertyName = model.IdPropertyName,
                    GenerateIdPropertyValue = model.GenerateIdPropertyValue,
                    PreExistingNodeUriPrefix = model.PreExistingNodeUriPrefix,
                    VisualiserNodeDepth = model.VisualiserNodeDepth,
                    VisualiserIncomingRelationshipsPathLength = model.VisualiserIncomingRelationshipsPathLength
                });
            }

            return Edit(contentTypePartDefinition);
        }

        private bool Valid(GraphSyncPartSettingsViewModel model, UpdateTypePartEditorContext context)
        {
            bool valid = true;

            if (model.VisualiserNodeDepth != null && model.VisualiserNodeDepth < 1)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(GraphSyncPartSettings.VisualiserNodeDepth),
                    "Visualiser node depth must be greater than 0");
                valid = false;
            }

            if (model.VisualiserIncomingRelationshipsPathLength != null && model.VisualiserIncomingRelationshipsPathLength < 1)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(GraphSyncPartSettings.VisualiserIncomingRelationshipsPathLength),
                    "Visualiser incoming relationships path length must be greater than 0");
                valid = false;
            }

            return valid;
        }

        private bool IsEqual(string? item, string? model)
        {
            return ((string.IsNullOrWhiteSpace(model) && string.IsNullOrWhiteSpace(item)) || model != null && model.Equals(item, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
