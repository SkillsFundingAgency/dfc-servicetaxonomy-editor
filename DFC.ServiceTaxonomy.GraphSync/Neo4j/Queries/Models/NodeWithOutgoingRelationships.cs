using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Model;
using Neo4j.Driver;
using OrchardCore.Workflows.Helpers;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models
{
    //todo: generic enough for neo4j project
    public class NodeWithOutgoingRelationships : INodeWithOutgoingRelationships
    {
        public INode SourceNode { get; set; }
        public IEnumerable<IOutgoingRelationship> OutgoingRelationships { get; set; }

        public NodeWithOutgoingRelationships(INode sourceNode, IEnumerable<(IRelationship relationship, INode destinationNode)> outgoingRelationships)
        {
            SourceNode = sourceNode;
            //todo: can we get null?
            //OutgoingRelationships = outgoingRelationships ?? new List<IOutgoingRelationship>();
            OutgoingRelationships =
                outgoingRelationships.Select(or => new OutgoingRelationship(or.relationship, or.destinationNode));
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

            IReplaceRelationshipsCommand replaceRelationshipsCommand = new ReplaceRelationshipsCommand
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


        //todo: this belongs in a derived class in graph sync, with the current command in neo4j
        /// <summary>
        /// Creates the command with the id's set appropriately for the supplied contentItemVersions
        /// </summary>
        public IReplaceRelationshipsCommand ToReplaceRelationshipsCommand(
            ISyncNameProvider syncNameProvider,
            IContentItemVersion fromContentItemVersion,
            IContentItemVersion toContentItemVersion,
            bool replaceExistingRelationships = true)
        {
            string sourceIdPropertyName = syncNameProvider.IdPropertyNameFromNodeLabels(SourceNode.Labels);

            IReplaceRelationshipsCommand replaceRelationshipsCommand = new ReplaceRelationshipsCommand
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
