using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Activities.Events;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Environment.Cache;
using OrchardCore.Workflows.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Custom
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
            var typeDefinition = _ocContentDefinitionManager.GetTypeDefinition(name);

            //Part being deleted is also type.
            if (typeDefinition != null)
            {
                _workflowManager.TriggerEventAsync(nameof(ContentTypeDeletedEvent), new { ContentType = name }, name).GetAwaiter().GetResult();
            }
            else
            {
                _ocContentDefinitionManager.DeletePartDefinition(name);
            }
        }

        public void DeleteTypeDefinition(string name)
        {
            //_ocContentDefinitionManager.DeleteTypeDefinition(name);
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
            _ocContentDefinitionManager.StorePartDefinition(contentPartDefinition);
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition)
        {
            _ocContentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);
        }
    }
}
