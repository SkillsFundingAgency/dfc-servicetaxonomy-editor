using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Events;
using OrchardCore.ContentTypes.Services;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Workflows.Services;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Activities.Events;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class CustomContentDefinitionService : IContentDefinitionService
    {
        private readonly ContentDefinitionService _contentDefinitionService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IWorkflowManager _workflowManager;

        public CustomContentDefinitionService(IContentDefinitionManager contentDefinitionManager,
                IEnumerable<IContentDefinitionEventHandler> contentDefinitionEventHandlers,
                IEnumerable<ContentPart> contentParts,
                IEnumerable<ContentField> contentFields,
                IOptions<ContentOptions> contentOptions,
                ILogger<IContentDefinitionService> logger,
                IStringLocalizer<ContentDefinitionService> localizer,
                IWorkflowManager workflowManager)
        {
            this._contentDefinitionService = new ContentDefinitionService(contentDefinitionManager, contentDefinitionEventHandlers, contentParts, contentFields, contentOptions, logger, localizer);
            _contentDefinitionManager = contentDefinitionManager;
            _workflowManager = workflowManager;
        }

        public void AddFieldToPart(string fieldName, string fieldTypeName, string partName)
        {
            _contentDefinitionService.AddFieldToPart(fieldName, fieldTypeName, partName);
        }

        public void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName)
        {
            _contentDefinitionService.AddFieldToPart(fieldName, displayName, fieldTypeName, partName);
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
            var definition = _contentDefinitionManager.GetTypeDefinition(name);

            if (definition.Parts.Any(x => string.Equals(x.Name, "GraphSyncPart", System.StringComparison.OrdinalIgnoreCase)))
            {
                _workflowManager.TriggerEventAsync(nameof(ContentTypeDeletedEvent), new { ContentType = name }, name).GetAwaiter().GetResult();
            }
            else
            {
                //It's not Synced to Graph, just delete it
                //_ocContentDefinitionManager.DeleteTypeDefinition(name);
            }
            //_contentDefinitionService.RemoveType(name, deleteContent);
        }
    }
}
