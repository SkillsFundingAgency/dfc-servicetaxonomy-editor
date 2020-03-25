using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Managers.Interface;
using Microsoft.Extensions.Primitives;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Managers
{
    public class CustomContentDefinitionManager : ICustomContentDefintionManager
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionStore _contentDefinitionStore;

        public CustomContentDefinitionManager(IContentDefinitionManager contentDefinitionManager, IContentDefinitionStore contentDefinitionStore)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionStore = contentDefinitionStore;
        }

        public IChangeToken ChangeToken => _contentDefinitionManager.ChangeToken;

        public void DeletePartDefinition(string name)
        {
            _contentDefinitionManager.DeletePartDefinition(name);
        }

        public void DeleteTypeDefinition(string name)
        {
            _contentDefinitionManager.DeleteTypeDefinition(name);
        }

        public ContentPartDefinition GetPartDefinition(string name)
        {
            //Bypass caching as caches are scoped
            return Build(LoadContentDefinitionRecord().ContentPartDefinitionRecords.FirstOrDefault(x => x.Name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)));
        }

        public ContentTypeDefinition GetTypeDefinition(string name)
        {
            //Bypass caching as caches are scoped
            return Build(LoadContentDefinitionRecord().ContentTypeDefinitionRecords.FirstOrDefault(x => x.Name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)), LoadContentDefinitionRecord().ContentPartDefinitionRecords);
        }

        public async Task<int> GetTypesHashAsync()
        {
            return await _contentDefinitionManager.GetTypesHashAsync();
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions()
        {
            return _contentDefinitionManager.ListPartDefinitions();
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions()
        {
            return _contentDefinitionManager.ListTypeDefinitions();
        }

        public ContentPartDefinition LoadPartDefinition(string name)
        {
            //Bypass caching as caches are scoped
            return Build(LoadContentDefinitionRecord().ContentPartDefinitionRecords.FirstOrDefault(x => x.Name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)));
        }

        public IEnumerable<ContentPartDefinition> LoadPartDefinitions()
        {
            return _contentDefinitionManager.LoadPartDefinitions();
        }

        public ContentTypeDefinition LoadTypeDefinition(string name)
        {
            //Bypass caching as caches are scoped
            return Build(LoadContentDefinitionRecord().ContentTypeDefinitionRecords.FirstOrDefault(x => x.Name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)), LoadContentDefinitionRecord().ContentPartDefinitionRecords);
        }

        public IEnumerable<ContentTypeDefinition> LoadTypeDefinitions()
        {
            return _contentDefinitionManager.LoadTypeDefinitions();
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition)
        {
            _contentDefinitionManager.StorePartDefinition(contentPartDefinition);
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition)
        {
            _contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);
        }

        /// <summary>
        /// Returns the document from the store to be updated.
        /// </summary>
        private ContentDefinitionRecord LoadContentDefinitionRecord() =>
            _contentDefinitionStore.LoadContentDefinitionAsync().GetAwaiter().GetResult();

        //TODO: Handle pragma warnings in Build methods. Lifted directly from OC
        private ContentTypeDefinition Build(ContentTypeDefinitionRecord source, IList<ContentPartDefinitionRecord> partDefinitionRecords)
        {
            if (source == null)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
            }

            var contentTypeDefinition = new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitionRecords.Select(tp => Build(tp, partDefinitionRecords.FirstOrDefault(p => p.Name == tp.PartName))),
                source.Settings);

            return contentTypeDefinition;
        }

        private ContentTypePartDefinition Build(ContentTypePartDefinitionRecord source, ContentPartDefinitionRecord partDefinitionRecord)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return source == null ? null : new ContentTypePartDefinition(
                source.Name,
                Build(partDefinitionRecord) ?? new ContentPartDefinition(source.PartName, Enumerable.Empty<ContentPartFieldDefinition>(), new JObject()),
                source.Settings);
#pragma warning restore CS8603 // Possible null reference return.
        }

        private ContentPartDefinition Build(ContentPartDefinitionRecord source)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return source == null ? null : new ContentPartDefinition(
                source.Name,
                source.ContentPartFieldDefinitionRecords.Select(Build),
                source.Settings);
#pragma warning restore CS8603 // Possible null reference return.
        }

        private ContentPartFieldDefinition Build(ContentPartFieldDefinitionRecord source)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return source == null ? null : new ContentPartFieldDefinition(
                Build(new ContentFieldDefinitionRecord { Name = source.FieldName }),
                source.Name,
                source.Settings
            );
#pragma warning restore CS8603 // Possible null reference return.
        }

        private ContentFieldDefinition Build(ContentFieldDefinitionRecord source)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return source == null ? null : new ContentFieldDefinition(source.Name);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
