using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class BagPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IServiceProvider _serviceProvider;

        public string? PartName => nameof(BagPart);

        public BagPartGraphSyncer(
            IGraphDatabase graphDatabase,
            IServiceProvider serviceProvider)
        {
            _graphDatabase = graphDatabase;
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

                IMergeNodeCommand? mergeNodeCommand = await graphSyncer.SyncToGraph(
                    contentItem!["ContentType"]!.ToString(), contentItem!);
                // only sync the content type contained in the bag if it has a graph lookup part
                if (mergeNodeCommand == null)
                    continue;

                var replaceRelationshipsCommand = _serviceProvider.GetRequiredService<IReplaceRelationshipsCommand>();
                //todo: instead of passing nodeProperties (and nodeRelationships) pass the node (& relationship) command
                // can then pick these out of the node command
                // will probably have to either: make sure graph sync part is run first
                // ^^ probably best to dupe idpropertyvalue in the command and ignore from properties collection (special case)
                // or let part syncers supply int priority/order <- not nice, better if syncers totally independent of each other (low coupling)
                //todo: helper on IReplaceRelationshipsCommand for this?
                //todo: major hackalert!!!
                replaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(new[] {"ncs__JobProfile"});
                replaceRelationshipsCommand.SourceIdPropertyName = "uri";
                replaceRelationshipsCommand.SourceIdPropertyValue = (string?)nodeProperties[replaceRelationshipsCommand.SourceIdPropertyName];

                //todo: hackalert! pick relationshiptype from hint
                // no hint for bag (or named bag parts)
                // also, might want 1 relationships for all content types, i.e. ncs__hasEntryRoute (name in content type also contains how to become)
                // or 1 relationship per content type, e.g. ncs__hasUniversityRoute
                // can we add something to the contained content for the relationship?
                string relationshipType = "ncs__hasBagPart";

                //todo: hackalert! destNodeLabel should be a set of labels
                string destNodeLabel = mergeNodeCommand.NodeLabels.First(l => l != "Resource");
                //todo: more thought to null handling
                replaceRelationshipsCommand.Relationships.Add((destNodeLabel, mergeNodeCommand.IdPropertyName!, relationshipType),
                    //todo: the types should match. string[] to object[]?
                    new[] {(string)mergeNodeCommand.Properties[mergeNodeCommand.IdPropertyName!]});

                await _graphDatabase.RunWriteQueries(replaceRelationshipsCommand.Query);
            }
        }
    }
}
