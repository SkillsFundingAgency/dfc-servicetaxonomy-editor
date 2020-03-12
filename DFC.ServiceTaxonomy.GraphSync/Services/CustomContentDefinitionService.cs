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

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class CustomContentDefinitionService : IContentDefinitionService
    {
        private readonly IOrchardCoreContentDefinitionService _contentDefinitionService;
        private readonly IWorkflowManager _workflowManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardCoreContentDefinitionService _orchardCoreContentDefinitionService;

        public CustomContentDefinitionService(IOrchardCoreContentDefinitionService contentDefinitionService, IWorkflowManager workflowManager, IContentDefinitionManager contentDefinitionManager, IOrchardCoreContentDefinitionService orchardCoreContentDefinitionService)
        {
            _contentDefinitionService = contentDefinitionService;
            _workflowManager = workflowManager;
            _contentDefinitionManager = contentDefinitionManager;
            _orchardCoreContentDefinitionService = orchardCoreContentDefinitionService;
        }

        public void AddFieldToPart(string fieldName, string fieldTypeName, string partName)
        {
            _contentDefinitionService.AddFieldToPart(fieldName, fieldTypeName, partName);
        }

        public void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName)
        {
            _contentDefinitionService.AddFieldToPart(fieldTypeName, displayName, fieldTypeName, partName);
        }

        public EditPartViewModel AddPart(CreatePartViewModel partViewModel)
        {
            return _contentDefinitionService.AddPart(partViewModel);
        }

        public void AddPartToType(string partName, string typeName)
        {
            _contentDefinitionService.AddPartToType(partName, typeName);
        }

        public void AddReusablePartToType(string name, string displayName, string description, string partName, string typeName)
        {
            _contentDefinitionService.AddReusablePartToType(name, displayName, description, partName, typeName);
        }

        public ContentTypeDefinition AddType(string name, string displayName)
        {
            return _contentDefinitionService.AddType(name, displayName);
        }

        public void AlterField(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel)
        {
            _contentDefinitionService.AlterField(partViewModel, fieldViewModel);
        }

        public void AlterPartFieldsOrder(ContentPartDefinition partDefinition, string[] fieldNames)
        {
            _contentDefinitionService.AlterPartFieldsOrder(partDefinition, fieldNames);
        }

        public void AlterTypePart(EditTypePartViewModel partViewModel)
        {
            _contentDefinitionService.AlterTypePart(partViewModel);
        }

        public void AlterTypePartsOrder(ContentTypeDefinition typeDefinition, string[] partNames)
        {
            _contentDefinitionService.AlterTypePartsOrder(typeDefinition, partNames);
        }

        public string GenerateContentTypeNameFromDisplayName(string displayName)
        {
            return _contentDefinitionService.GenerateContentTypeNameFromDisplayName(displayName);
        }

        public string GenerateFieldNameFromDisplayName(string partName, string displayName)
        {
            return _contentDefinitionService.GenerateFieldNameFromDisplayName(partName, displayName);
        }

        public IEnumerable<Type> GetFields()
        {
            return _contentDefinitionService.GetFields();
        }

        public EditPartViewModel GetPart(string name)
        {
            return _contentDefinitionService.GetPart(name);
        }

        public IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly)
        {
            return _contentDefinitionService.GetParts(metadataPartsOnly);
        }

        public EditTypeViewModel GetType(string name)
        {
            return _contentDefinitionService.GetType(name);
        }

        public IEnumerable<EditTypeViewModel> GetTypes()
        {
            return _contentDefinitionService.GetTypes();
        }

        public EditPartViewModel LoadPart(string name)
        {
            return _contentDefinitionService.LoadPart(name);
        }

        public IEnumerable<EditPartViewModel> LoadParts(bool metadataPartsOnly)
        {
            return _contentDefinitionService.LoadParts(metadataPartsOnly);
        }

        public EditTypeViewModel LoadType(string name)
        {
            return _contentDefinitionService.LoadType(name);
        }

        public IEnumerable<EditTypeViewModel> LoadTypes()
        {
            return _contentDefinitionService.LoadTypes();
        }

        public void RemoveFieldFromPart(string fieldName, string partName)
        {
            _contentDefinitionService.RemoveFieldFromPart(fieldName, partName);
        }

        public void RemovePart(string name)
        {
            _contentDefinitionService.RemovePart(name);
        }

        public void RemovePartFromType(string partName, string typeName)
        {
            _contentDefinitionService.RemovePartFromType(partName, typeName);
        }

        public void RemoveType(string name, bool deleteContent)
        {
            var typeBeingDeleted = _contentDefinitionManager.GetTypeDefinition(name);

            if (typeBeingDeleted.Parts.Any(x => x.Name == "GraphSyncPart"))
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
