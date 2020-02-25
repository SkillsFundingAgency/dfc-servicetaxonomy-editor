using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
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

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IGraphSyncPartIdProperty _graphSyncPartIdProperty;
        private readonly IServiceProvider _serviceProvider;

        public string? PartName => nameof(BagPart);

        public BagPartGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IGraphSyncPartIdProperty graphSyncPartIdProperty,
            IServiceProvider serviceProvider)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<Query>> AddSyncComponents(
            dynamic graphLookupContent,
            IDictionary<string, object> nodeProperties,
            //IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            var queries = new List<Query>();

            foreach (JObject? contentItem in graphLookupContent.ContentItems)
            {
                var graphSyncer = _serviceProvider.GetRequiredService<IGraphSyncer>();

                string contentType = contentItem!["ContentType"]!.ToString();
                //todo: if we want to support nested bags, would have to return queries also
                //document nested bags in xmldoc
                IMergeNodeCommand? mergeNodeCommand = await graphSyncer.SyncToGraph(contentType, contentItem!);
                // only sync the content type contained in the bag if it has a graph lookup part
                if (mergeNodeCommand == null)
                    continue;

                var delayedReplaceRelationshipsCommand = _serviceProvider.GetRequiredService<IReplaceRelationshipsCommand>();
                //todo: instead of passing nodeProperties (and nodeRelationships) pass the node (& relationship) command
                // can then pick these out of the node command
                // will probably have to either: make sure graph sync part is run first
                // ^^ probably best to dupe idpropertyvalue in the command and ignore from properties collection (special case)
                // or let part syncers supply int priority/order <- not nice, better if syncers totally independent of each other (low coupling)
                // ^^ add to graph sync part settings: bag content item relationship
                //todo: helper on IReplaceRelationshipsCommand for this?
                //todo: won't work for nested bags?
                delayedReplaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(
                    new[] {NcsPrefix + contentTypePartDefinition.ContentTypeDefinition.Name});
                //todo: get from correct graph sync settings
                delayedReplaceRelationshipsCommand.SourceIdPropertyName = _graphSyncPartIdProperty.Name;
                delayedReplaceRelationshipsCommand.SourceIdPropertyValue = (string?)nodeProperties[delayedReplaceRelationshipsCommand.SourceIdPropertyName];

                var graphSyncPartSettings = GetGraphSyncPartSettings(contentType);
                string? relationshipType = graphSyncPartSettings.BagPartContentItemRelationshipType;
                if (string.IsNullOrEmpty(relationshipType))
                    relationshipType = "ncs__hasBagPart";

                //todo: hackalert! destNodeLabel should be a set of labels
                //string destNodeLabel = mergeNodeCommand.NodeLabels.First(l => l != "Resource");
                //todo: more thought to null handling
                //replaceRelationshipsCommand.Relationships.Add((destNodeLabel, mergeNodeCommand.IdPropertyName!, relationshipType),
                //todo: the types should match. string[] to object[]?
                delayedReplaceRelationshipsCommand.AddRelationshipsTo(
                    relationshipType,
                    mergeNodeCommand.NodeLabels,
                    mergeNodeCommand.IdPropertyName!,
                    (string)mergeNodeCommand.Properties[mergeNodeCommand.IdPropertyName!]);

                queries.Add(delayedReplaceRelationshipsCommand.Query);
            }

            return queries;
        }

        private GraphSyncPartSettings GetGraphSyncPartSettings(string contentType)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(GraphSyncPart));
            return contentTypePartDefinition.GetSettings<GraphSyncPartSettings>();
        }
    }
}
