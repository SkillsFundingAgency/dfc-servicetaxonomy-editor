using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Commands;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using OrchardCore.Workflows.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.GraphSync.JsonConverters;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries.Models
{
    [JsonConverter(typeof(NoConverter))]
    public class CosmosDbNodeWithOutgoingRelationships : INodeWithOutgoingRelationships
    {
        public INode SourceNode { get; set; }
        #pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        public IEnumerable<IOutgoingRelationship>? OutgoingRelationships { get; set; }
        #pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

        public CosmosDbNodeWithOutgoingRelationships(INode sourceNode, IEnumerable<(IRelationship relationship, INode destinationNode)> outgoingRelationships)
        {
            SourceNode = sourceNode;
            //todo: can we get null?
            //OutgoingRelationships = outgoingRelationships ?? new List<IOutgoingRelationship>(); ANSWER - YES
            OutgoingRelationships =
                outgoingRelationships?.Select(or => new OutgoingRelationship(or.relationship, or.destinationNode));
        }

        #pragma warning disable S4136
        public IEnumerable<CommandRelationship> ToCommandRelationships(ISyncNameProvider syncNameProvider)
        {
            //todo: don't get id twice
            var commandRelationshipGroups = OutgoingRelationships.GroupBy(
                or => new CommandRelationship(
                    or.Relationship.Type,
                    null,
                    or.Relationship.Properties,
                    or.DestinationNode.Labels,
                    syncNameProvider.IdPropertyNameFromNodeLabels(or.DestinationNode.Labels),
                    null),
                or => or.DestinationNode.Properties[
                    syncNameProvider.IdPropertyNameFromNodeLabels(or.DestinationNode.Labels)]);

            return commandRelationshipGroups.Select(g =>
            {
                g.Key.DestinationNodeIdPropertyValues.AddRange(g);
                return g.Key;
            });
        }

        public IReplaceRelationshipsCommand ToReplaceRelationshipsCommand(ISyncNameProvider syncNameProvider)
        {
            string sourceIdPropertyName = syncNameProvider.IdPropertyNameFromNodeLabels(SourceNode.Labels);

            IReplaceRelationshipsCommand replaceRelationshipsCommand = new CosmosDbReplaceRelationshipsCommand
            {
                SourceIdPropertyName = sourceIdPropertyName,
                SourceIdPropertyValue = SourceNode.Properties[sourceIdPropertyName],
                SourceNodeLabels = new HashSet<string>(SourceNode.Labels)
            };

            //todo: twoway
            replaceRelationshipsCommand.AddRelationshipsTo(ToCommandRelationships(syncNameProvider));

            return replaceRelationshipsCommand;
        }

        //todo: this belongs in a derived class in graph sync, with the current command in neo4j
        public IEnumerable<CommandRelationship> ToCommandRelationships(
            ISyncNameProvider syncNameProvider,
            IContentItemVersion fromContentItemVersion,
            IContentItemVersion toContentItemVersion)
        {
            //todo: don't get id twice
            var commandRelationshipGroups = OutgoingRelationships.GroupBy(
                or => new CommandRelationship(
                    or.Relationship.Type,
                    null,
                    or.Relationship.Properties,
                    or.DestinationNode.Labels,
                    syncNameProvider.IdPropertyNameFromNodeLabels(or.DestinationNode.Labels),
                    null),
                or => or.DestinationNode.Properties[
                    syncNameProvider.IdPropertyNameFromNodeLabels(or.DestinationNode.Labels)]);

            return commandRelationshipGroups.Select(g =>
            {
                var toContentItemVersionIds = g
                    .Select(fromContentItemVersionId => syncNameProvider.ConvertIdPropertyValue(
                        (string)fromContentItemVersionId, toContentItemVersion, fromContentItemVersion));
                g.Key.DestinationNodeIdPropertyValues.AddRange(toContentItemVersionIds);
                return g.Key;
            });
        }


        /// <summary>
        /// Creates the command with the id's set appropriately for the supplied contentItemVersions
        /// </summary>
        public IReplaceRelationshipsCommand ToReplaceRelationshipsCommand(
            ISyncNameProvider syncNameProvider,
            IContentItemVersion fromContentItemVersion,
            IContentItemVersion toContentItemVersion,
            bool replaceExistingRelationships = true)
        {
            if (SourceNode == null)
            {
                return new CosmosDbReplaceRelationshipsCommand();
            }

            string sourceIdPropertyName = syncNameProvider.IdPropertyNameFromNodeLabels(SourceNode.Labels);

            IReplaceRelationshipsCommand replaceRelationshipsCommand = new CosmosDbReplaceRelationshipsCommand
            {
                SourceIdPropertyName = sourceIdPropertyName,
                SourceIdPropertyValue = syncNameProvider.ConvertIdPropertyValue(
                    (string)SourceNode.Properties[sourceIdPropertyName],
                    toContentItemVersion,
                    fromContentItemVersion),
                SourceNodeLabels = new HashSet<string>(SourceNode.Labels),
                ReplaceExistingRelationships = replaceExistingRelationships
            };

            //todo: twoway
            replaceRelationshipsCommand.AddRelationshipsTo(ToCommandRelationships(
                syncNameProvider,
                fromContentItemVersion,
                toContentItemVersion));

            return replaceRelationshipsCommand;
        }
        #pragma warning restore S4136
    }
}
