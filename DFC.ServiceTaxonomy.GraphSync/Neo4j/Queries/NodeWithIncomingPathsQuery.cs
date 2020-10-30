using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries
{
    /*

     https://prezi.com/p/owhz2icrbmby/apoc-path-expander-procedures/
     https://stackoverflow.com/questions/44248154/how-extract-the-complete-trees-in-order-with-cypher


match (n:SharedContent)
  where n.uri = 'http://localhost:7071/api/execute/sharedcontent/0fc70da1-ed83-495d-8972-d9a78c1973c5'

call apoc.path.expand(n, "<", null, 0, 2)
yield path

with apoc.path.elements(path) as elements
return elements[0] as sourceNode, collect({firstHopRelationship: elements[1], firstHopNodes: elements[2], hop2Relationship: elements[3], hop2Nodes: elements[4]}) as incoming

//todo: sourceNode, {rel, node, {rel, node, {rel, node, null}}}
//rather than sourcenode {r1, n1, r2, n2}
// how to return in the hierarchy we'd have to construct from the returns?



Option A

easier to generate cypher (but relationships and nodes not labelled)

match (n:SharedContent)
  where n.uri = //'http://localhost:7071/api/execute/sharedcontent/dfde0410-b1ce-414e-ac2f-f3cffeac4f9b' // no incoming
  'http://localhost:7071/api/execute/sharedcontent/0fc70da1-ed83-495d-8972-d9a78c1973c5' // 2 degree incoming

call apoc.path.expand(n, "<", null, 0, 2)
yield path

with apoc.path.elements(path) as elements
return elements[0] as sourceNode, collect(elements[1..]) as incoming






match (n:SharedContent)
  where n.uri = 'http://localhost:7071/api/execute/sharedcontent/0fc70da1-ed83-495d-8972-d9a78c1973c5'

call apoc.path.expand(n, "<", null, 0, 2)
yield path

with apoc.path.elements(path) as elements
with elements[0] as sourceNode, collect(distinct {firstHopRelationship: elements[1], firstHopNodes: elements[2]}) as level1, elements
with sourceNode, [l1 in level1 | {rel: l1.firstHopRelationship, node: l1.firstHopNode, next: collect(distinct {rel: elements[3], nod: elements[4]}) where elements[1].uri = l1.firstHopNode.uri }]
//  ^^^^ this bit
return sourceNode, level1, xxx



//spanning tree?

match (n:SharedContent)
  where n.uri = 'http://localhost:7071/api/execute/sharedcontent/0fc70da1-ed83-495d-8972-d9a78c1973c5'

call apoc.path.spanningTree(n, {maxLevel: 2, relationshipFilter: '<'})
yield path

with apoc.path.elements(path) as elements
return elements


// subgraphAll (option B)

match (n:SharedContent)
  where n.uri = 'http://localhost:7071/api/execute/sharedcontent/0fc70da1-ed83-495d-8972-d9a78c1973c5'

call apoc.path.subgraphAll(n, {maxLevel: 2, relationshipFilter: '<'})
yield nodes, relationships

return nodes, relationships


Option A would be better to create a hierarchy for validation and such like

Option B is probably beter for visualisation, as its what we massage the data into for the visualiser anyway

     */


    //todo: replace usage of NodeWithIncomingRelationshipsQuery with this more general case
    public class NodeWithIncomingPathsQuery : IQuery<INodeWithIncomingRelationships?>
    {
        private int MaxPathLength { get; }
        private IEnumerable<string> NodeLabels { get; }
        private string IdPropertyName { get; }
        private object IdPropertyValue { get; }

        public NodeWithIncomingPathsQuery(
            IEnumerable<string> nodeLabels,
            string idPropertyName,
            object idPropertyValue,
            int maxPathLength = 1)
        {
            MaxPathLength = maxPathLength;
            NodeLabels = nodeLabels;
            IdPropertyName = idPropertyName;
            IdPropertyValue = idPropertyValue;
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if (!NodeLabels.Any())
            {
                validationErrors.Add("At least one NodeLabel must be provided.");
            }

            if (MaxPathLength < 0)
            {
                validationErrors.Add("MaxPathLength must not be negative.");
            }

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                var parameters = new Dictionary<string, object>
                {
                    {"maxLevel", MaxPathLength},
                    {"idPropertyValue", IdPropertyValue}
                };

                return new Query(
                    @$"match (n:{string.Join(":", NodeLabels)} {{{IdPropertyName}:$idPropertyValue}})
call apoc.path.subgraphAll(n, {{maxLevel: $maxLevel, relationshipFilter: '<'}}) yield nodes, relationships
return nodes, relationships", parameters);

            }
        }

        //todo: need to update to handle multi path
        public INodeWithIncomingRelationships? ProcessRecord(IRecord record)
        {
            var results = (Dictionary<string, object>)record["sourceNodeWithIncomingRelationships"];
            if (results == null)
                throw new QueryResultException($"{nameof(NodeWithIncomingRelationshipsQuery)} results not in expected format.");

            if (!(results["sourceNode"] is INode sourceNode))
                return null;

            IEnumerable<(IRelationship, INode)> incomingRelationships =
                ((IEnumerable<object>)results["incomingRelationships"])
                .Cast<IDictionary<string, object>>()
                .Select(or =>
                    ((IRelationship)or["relationship"], (INode)or["destinationNode"]));

            if (incomingRelationships.Count() == 1 && incomingRelationships.First().Item1 == null)
                incomingRelationships = Enumerable.Empty<(IRelationship, INode)>();

            INodeWithIncomingRelationships nodeWithIncoming =
                //todo: check all combos of missing data
                new NodeWithIncomingRelationships(sourceNode, incomingRelationships);

            return nodeWithIncoming;
        }
    }
}
