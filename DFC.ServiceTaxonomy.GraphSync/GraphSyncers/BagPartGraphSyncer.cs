using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
//using DFC.ServiceTaxonomy.GraphSync.Models;
//using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class BagPartGraphSyncer : IContentPartGraphSyncer
    {
        //todo: have as setting (add prefix to namespace settings?)
        private const string NcsPrefix = "ncs__";

        private readonly IGraphDatabase _graphDatabase;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IGraphSyncPartIdProperty _graphSyncPartIdProperty;
        private readonly IServiceProvider _serviceProvider;

        public string? PartName => nameof(BagPart);

        public BagPartGraphSyncer(
            IGraphDatabase graphDatabase,
            IContentDefinitionManager contentDefinitionManager,
            IGraphSyncPartIdProperty graphSyncPartIdProperty,
            IServiceProvider serviceProvider)
        {
            _graphDatabase = graphDatabase;
            _contentDefinitionManager = contentDefinitionManager;
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
            _serviceProvider = serviceProvider;
        }

        public async Task AddSyncComponents(
            dynamic graphLookupContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            foreach (JObject? contentItem in graphLookupContent.ContentItems)
            {
                var graphSyncer = _serviceProvider.GetRequiredService<IGraphSyncer>();

                string contentType = contentItem!["ContentType"]!.ToString();
                IMergeNodeCommand? mergeNodeCommand = await graphSyncer.SyncToGraph(contentType, contentItem!);
                // only sync the content type contained in the bag if it has a graph lookup part
                if (mergeNodeCommand == null)
                    continue;

                var replaceRelationshipsCommand = _serviceProvider.GetRequiredService<IReplaceRelationshipsCommand>();
                //todo: instead of passing nodeProperties (and nodeRelationships) pass the node (& relationship) command
                // can then pick these out of the node command
                // will probably have to either: make sure graph sync part is run first
                // ^^ probably best to dupe idpropertyvalue in the command and ignore from properties collection (special case)
                // or let part syncers supply int priority/order <- not nice, better if syncers totally independent of each other (low coupling)
                // ^^ add to graph sync part settings: bag content item relationship
                //todo: helper on IReplaceRelationshipsCommand for this?
                //todo: won't work for nested bags?
                replaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(new[] {NcsPrefix + contentTypePartDefinition.ContentTypeDefinition.Name});
                //todo: get from correct graph sync settings
                replaceRelationshipsCommand.SourceIdPropertyName = _graphSyncPartIdProperty.Name;
                replaceRelationshipsCommand.SourceIdPropertyValue = (string?)nodeProperties[replaceRelationshipsCommand.SourceIdPropertyName];

                var graphSyncPartSettings = GetGraphSyncPartSettings(contentType);
                string? relationshipType = graphSyncPartSettings.BagPartContentItemRelationshipType;
                if (string.IsNullOrEmpty(relationshipType))
                    relationshipType = "ncs__hasBagPart";

                //todo: hackalert! destNodeLabel should be a set of labels
                string destNodeLabel = mergeNodeCommand.NodeLabels.First(l => l != "Resource");
                //todo: more thought to null handling
                replaceRelationshipsCommand.Relationships.Add((destNodeLabel, mergeNodeCommand.IdPropertyName!, relationshipType),
                    //todo: the types should match. string[] to object[]?
                    new[] {(string)mergeNodeCommand.Properties[mergeNodeCommand.IdPropertyName!]});

                await _graphDatabase.RunWriteQueries(replaceRelationshipsCommand.Query);
            }
        }

        private GraphSyncPartSettings GetGraphSyncPartSettings(string contentType)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphSyncPart));
            return contentTypePartDefinition.GetSettings<GraphSyncPartSettings>();
        }
    }
}
