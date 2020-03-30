using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Services;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Workflows.Services;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Activities.Events;
using OrchardCore.ContentManagement.Metadata;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class CustomContentDefinitionService : IContentDefinitionService
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardCoreContentDefinitionService _orchardCoreContentDefinitionService;

        public CustomContentDefinitionService(IOrchardCoreContentDefinitionService contentDefinitionService, IWorkflowManager workflowManager, IContentDefinitionManager contentDefinitionManager, IOrchardCoreContentDefinitionService orchardCoreContentDefinitionService)
        {
            _workflowManager = workflowManager;
            _contentDefinitionManager = contentDefinitionManager;
            _orchardCoreContentDefinitionService = orchardCoreContentDefinitionService;
        }

        public void AddFieldToPart(string fieldName, string fieldTypeName, string partName)
        {
            _orchardCoreContentDefinitionService.AddFieldToPart(fieldName, fieldTypeName, partName);
        }

        public void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName)
        {
            _orchardCoreContentDefinitionService.AddFieldToPart(fieldName, displayName, fieldTypeName, partName);
        }

        public EditPartViewModel AddPart(CreatePartViewModel partViewModel)
        {
            return _orchardCoreContentDefinitionService.AddPart(partViewModel);
        }

        public void AddPartToType(string partName, string typeName)
        {
            _orchardCoreContentDefinitionService.AddPartToType(partName, typeName);
        }

        public void AddReusablePartToType(string name, string displayName, string description, string partName, string typeName)
        {
            _orchardCoreContentDefinitionService.AddReusablePartToType(name, displayName, description, partName, typeName);
        }

        public ContentTypeDefinition AddType(string name, string displayName)
        {
            return _orchardCoreContentDefinitionService.AddType(name, displayName);
        }

        public void AlterField(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel)
        {
            _orchardCoreContentDefinitionService.AlterField(partViewModel, fieldViewModel);
        }

        public void AlterPartFieldsOrder(ContentPartDefinition partDefinition, string[] fieldNames)
        {
            _orchardCoreContentDefinitionService.AlterPartFieldsOrder(partDefinition, fieldNames);
        }

        public void AlterTypePart(EditTypePartViewModel partViewModel)
        {
            _orchardCoreContentDefinitionService.AlterTypePart(partViewModel);
        }

        public void AlterTypePartsOrder(ContentTypeDefinition typeDefinition, string[] partNames)
        {
            _orchardCoreContentDefinitionService.AlterTypePartsOrder(typeDefinition, partNames);
        }

        public string GenerateContentTypeNameFromDisplayName(string displayName)
        {
            return _orchardCoreContentDefinitionService.GenerateContentTypeNameFromDisplayName(displayName);
        }

        public string GenerateFieldNameFromDisplayName(string partName, string displayName)
        {
            return _orchardCoreContentDefinitionService.GenerateFieldNameFromDisplayName(partName, displayName);
        }

        public IEnumerable<Type> GetFields()
        {
            return _orchardCoreContentDefinitionService.GetFields();
        }

        public EditPartViewModel GetPart(string name)
        {
            return _orchardCoreContentDefinitionService.GetPart(name);
        }

        public IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly)
        {
            return _orchardCoreContentDefinitionService.GetParts(metadataPartsOnly);
        }

        public EditTypeViewModel GetType(string name)
        {
            return _orchardCoreContentDefinitionService.GetType(name);
        }

        public IEnumerable<EditTypeViewModel> GetTypes()
        {
            return _orchardCoreContentDefinitionService.GetTypes();
        }

        public EditPartViewModel LoadPart(string name)
        {
            return _orchardCoreContentDefinitionService.LoadPart(name);
        }

        public IEnumerable<EditPartViewModel> LoadParts(bool metadataPartsOnly)
        {
            return _orchardCoreContentDefinitionService.LoadParts(metadataPartsOnly);
        }

        public EditTypeViewModel LoadType(string name)
        {
            return _orchardCoreContentDefinitionService.LoadType(name);
        }

        public IEnumerable<EditTypeViewModel> LoadTypes()
        {
            return _orchardCoreContentDefinitionService.LoadTypes();
        }

        public void RemoveFieldFromPart(string fieldName, string partName)
        {
            var typeBeingUpdated = _contentDefinitionManager.GetTypeDefinition(partName);

            if (typeBeingUpdated != null && typeBeingUpdated.Parts.Any(x => x.Name == nameof(GraphSyncPart)))
            {
                _contentDefinitionManager.AlterPartDefinition(partName, x => x.RemoveField(fieldName));
                _workflowManager.TriggerEventAsync(nameof(ContentTypeFieldRemovedEvent), new { ContentType = typeBeingUpdated.Name, RemovedField = fieldName }, typeBeingUpdated.Name).GetAwaiter().GetResult();
                return;
            }

            _contentDefinitionManager.AlterPartDefinition(partName, x => x.RemoveField(fieldName));
        }

        public void RemovePart(string name)
        {
            _orchardCoreContentDefinitionService.RemovePart(name);
        }

        public void RemovePartFromType(string partName, string typeName)
        {
            _orchardCoreContentDefinitionService.RemovePartFromType(partName, typeName);
        }

        public void RemoveType(string name, bool deleteContent)
        {
            var typeBeingDeleted = _contentDefinitionManager.GetTypeDefinition(name);

            if (typeBeingDeleted.Parts.Any(x => x.Name == nameof(GraphSyncPart)))
            {
                _workflowManager.TriggerEventAsync(nameof(ContentTypeDeletedEvent), new { ContentType = name }, name).GetAwaiter().GetResult();
            }
            else
            {
                _orchardCoreContentDefinitionService.RemoveType(name, deleteContent);
            }
        }
    }
}
