using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Activities.Events;
using DFC.ServiceTaxonomy.GraphSync.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Environment.Cache;
using OrchardCore.Workflows.Services;
using System.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Custom
{
    public class CustomContentDefinitionManager : IContentDefinitionManager
    {
        private readonly IOrchardCoreContentDefinitionManager _ocContentDefinitionManager;
        private readonly IWorkflowManager _workflowManager;

        public CustomContentDefinitionManager(
            ISignal signal,
            IContentDefinitionStore contentDefinitionStore,
            IOrchardCoreContentDefinitionManager orchardCoreDefinitionManager,
            IMemoryCache memoryCache,
            IWorkflowManager workflowManager)
        {
            _ocContentDefinitionManager = orchardCoreDefinitionManager;
            _workflowManager = workflowManager;
        }

        public IChangeToken ChangeToken => throw new System.NotImplementedException();

        public void DeletePartDefinition(string name)
        {
            _ocContentDefinitionManager.DeletePartDefinition(name);
        }

        public void DeleteTypeDefinition(string name)
        {
            var typeBeingDeleted = GetTypeDefinition(name);

            if (typeBeingDeleted.Parts.Any(x => x.Name == "GraphSyncPart"))
            {
                _workflowManager.TriggerEventAsync(nameof(ContentTypeDeletedEvent), new { ContentType = name }, name).GetAwaiter().GetResult();
            }
            else
            {
                _ocContentDefinitionManager.DeleteTypeDefinition(name);
            }
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
