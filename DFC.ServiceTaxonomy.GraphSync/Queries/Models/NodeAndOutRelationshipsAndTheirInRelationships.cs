using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Model;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Queries.Models
{
    public class NodeAndOutRelationshipsAndTheirInRelationships : INodeAndOutRelationshipsAndTheirInRelationships
    {
        public INode SourceNode { get; set; }
        //todo: new types for these rather than tuples
        // rename
        public IEnumerable<(IOutgoingRelationship, IEnumerable<IOutgoingRelationship>)> OutgoingRelationships { get; set; }

        public NodeAndOutRelationshipsAndTheirInRelationships(
            INode sourceNode,
            IEnumerable<(IRelationship relationship, INode destNode, IEnumerable<(IRelationship relationship, INode destNode)> destinationIncomingRelationships)>? outgoingRelationships)
        {
            SourceNode = sourceNode;
            //todo: can we get null?
            //OutgoingRelationships = outgoingRelationships ?? new List<IOutgoingRelationship>();
            OutgoingRelationships =
                (IEnumerable<(IOutgoingRelationship, IEnumerable<IOutgoingRelationship>)>)outgoingRelationships
                    .Select(or =>
                        (new OutgoingRelationship(or.relationship, or.destNode),
                            or.destinationIncomingRelationships.Select(ir =>
                                new OutgoingRelationship(ir.relationship, ir.destNode))));
        }

        public IEnumerable<CommandRelationship> ToCommandRelationships(IGraphSyncHelper graphSyncHelper)
        {
            throw new NotImplementedException();
            // //todo: don't get id twice
            // var commandRelationshipGroups = OutgoingRelationships.GroupBy(
            //     or => new CommandRelationship(
            //         or.Relationship.Type,
            //         or.Relationship.Properties,
            //         or.DestinationNode.Labels,
            //         graphSyncHelper.IdPropertyNameFromNodeLabels(or.DestinationNode.Labels),
            //         null),
            //     or => or.DestinationNode.Properties[
            //         graphSyncHelper.IdPropertyNameFromNodeLabels(or.DestinationNode.Labels)]);
            //
            // return commandRelationshipGroups.Select(g =>
            // {
            //     g.Key.DestinationNodeIdPropertyValues.AddRange(g);
            //     return g.Key;
            // });
        }
    }
}
