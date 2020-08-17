using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Model;
using Neo4j.Driver;
using OrchardCore.Workflows.Helpers;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models
{
    public class NodeAndOutRelationshipsAndTheirInRelationships : INodeAndOutRelationshipsAndTheirInRelationships
    {
        public INode SourceNode { get; set; }
        //todo: new types for these rather than tuples, or at least name
        //todo: rename
        //todo: IIncomingRelationship with common base, (or Relationship with flag??)
        public IEnumerable<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)> OutgoingRelationships { get; set; }

        public NodeAndOutRelationshipsAndTheirInRelationships(
            INode sourceNode,
            IEnumerable<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)> outgoingRelationships)
        {
            SourceNode = sourceNode;
            OutgoingRelationships = outgoingRelationships;
        }

        public NodeAndOutRelationshipsAndTheirInRelationships(
            INode sourceNode,
            IEnumerable<(IRelationship relationship, INode destNode, IEnumerable<(IRelationship relationship, INode destNode)> destinationIncomingRelationships)> outgoingRelationships)
        {
            SourceNode = sourceNode;
            //todo: can we get null?
            //OutgoingRelationships = outgoingRelationships ?? new List<IOutgoingRelationship>();
            OutgoingRelationships =
                outgoingRelationships
                    .Select(or =>
                        ((IOutgoingRelationship)new OutgoingRelationship(or.relationship, or.destNode),
                            or.destinationIncomingRelationships.Select(ir =>
                                (IOutgoingRelationship)new OutgoingRelationship(ir.relationship, ir.destNode))));
        }

        public IEnumerable<CommandRelationship> ToCommandRelationships(IGraphSyncHelper graphSyncHelper)
        {
            //todo: don't get id twice
            var commandRelationshipGroups = OutgoingRelationships.GroupBy(
                or => new CommandRelationship(
                    or.outgoingRelationship.Relationship.Type,
                    null,
                    or.outgoingRelationship.Relationship.Properties,
                    or.outgoingRelationship.DestinationNode.Labels,
                    graphSyncHelper.IdPropertyNameFromNodeLabels(or.outgoingRelationship.DestinationNode.Labels),
                    null),
                or => or.outgoingRelationship.DestinationNode.Properties[
                    graphSyncHelper.IdPropertyNameFromNodeLabels(or.outgoingRelationship.DestinationNode.Labels)]);

            return commandRelationshipGroups.Select(g =>
            {
                g.Key.DestinationNodeIdPropertyValues.AddRange(g);
                return g.Key;
            });
        }
    }
}
