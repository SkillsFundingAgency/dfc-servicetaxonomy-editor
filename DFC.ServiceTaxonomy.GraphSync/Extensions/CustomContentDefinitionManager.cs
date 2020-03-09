using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Environment.Cache;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Activities;
using System;
using OrchardCore.Workflows.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public class CustomContentDefinitionManager : IContentDefinitionManager
    {
        private readonly ContentDefinitionManager _ocContentDefinitionManager;
        private readonly IWorkflowManager _workflowManager;

        public CustomContentDefinitionManager(
            ISignal signal,
            IContentDefinitionStore contentDefinitionStore,
            IMemoryCache memoryCache,
            IWorkflowManager workflowManager)
        {
            _ocContentDefinitionManager = new ContentDefinitionManager(signal, contentDefinitionStore, memoryCache);
            _workflowManager = workflowManager;
        }

        public IChangeToken ChangeToken => throw new System.NotImplementedException();

        public void DeletePartDefinition(string name)
        {
            _ocContentDefinitionManager.DeletePartDefinition(name);
        }

        public void DeleteTypeDefinition(string name)
        {
            _ocContentDefinitionManager.DeleteTypeDefinition(name);
        }

        public ContentPartDefinition GetPartDefinition(string name)
        {
            return _ocContentDefinitionManager.GetPartDefinition(name);
        }

        public ContentTypeDefinition GetTypeDefinition(string name)
        {
            return _ocContentDefinitionManager.GetTypeDefinition(name);
        }

        public Task<int> GetTypesHashAsync()
        {
            return _ocContentDefinitionManager.GetTypesHashAsync();
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions()
        {
            return _ocContentDefinitionManager.ListPartDefinitions();
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions()
        {
            return _ocContentDefinitionManager.ListTypeDefinitions();
        }

        public ContentPartDefinition LoadPartDefinition(string name)
        {
            return _ocContentDefinitionManager.LoadPartDefinition(name);
        }

        public IEnumerable<ContentPartDefinition> LoadPartDefinitions()
        {
            return _ocContentDefinitionManager.LoadPartDefinitions();
        }

        public ContentTypeDefinition LoadTypeDefinition(string name)
        {
            return _ocContentDefinitionManager.LoadTypeDefinition(name);
        }

        public IEnumerable<ContentTypeDefinition> LoadTypeDefinitions()
        {
            return _ocContentDefinitionManager.LoadTypeDefinitions();
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition)
        {
            //var serializerSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var addedFields = new List<string>();
            var removedFields = new List<string>();

            var existingPartDefinition = this.GetPartDefinition(contentPartDefinition.Name);

            _ocContentDefinitionManager.StorePartDefinition(contentPartDefinition);

            foreach (var partField in contentPartDefinition.Fields)
            {
                var existingField = existingPartDefinition.Fields.FirstOrDefault(x => x.Name == partField.Name);

                if (existingField == null)
                {
                    //New field has been added
                    addedFields.Add(partField.Name);
                }

            }

            foreach (var partField in existingPartDefinition.Fields)
            {
                var newField = contentPartDefinition.Fields.FirstOrDefault(x => x.Name == partField.Name);

                if (newField == null)
                {
                    //Old field has been removed
                    removedFields.Add(partField.Name);
                }
            }

            _workflowManager.TriggerEventAsync(nameof(ContentTypeUpdatedEvent), new { Added = addedFields, Removed = removedFields }, contentPartDefinition.Name);
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition)
        {
            //var serializerSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };


            //var existingDefinition = this.GetTypeDefinition(contentTypeDefinition.Name);
            _ocContentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);

            //foreach (var part in existingdefinition.parts)
            //{
            //    var newparttocompare = contenttypedefinition.parts.firstordefault(x => x.name == part.name).partdefinition;

            //    var diff = finddiff(jsonconvert.serializeobject(part.partdefinition.fields, serializersettings), jsonconvert.serializeobject(newparttocompare.fields, serializersettings));


            //}


            try
            {

                _workflowManager.TriggerEventAsync(nameof(ContentTypeUpdatedEvent), new { NewDefinition = contentTypeDefinition, OldDefinition = contentTypeDefinition }, contentTypeDefinition.Name).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
#pragma warning disable S1481 // Unused local variables should be removed
                var a = _ocContentDefinitionManager.GetPartDefinition(ex.Message);
#pragma warning restore S1481 // Unused local variables should be removed
            }
        }

    }
}
