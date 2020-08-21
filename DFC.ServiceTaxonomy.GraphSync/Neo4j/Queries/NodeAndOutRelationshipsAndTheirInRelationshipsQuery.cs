using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries
{
    //todo: common base class with NoteWithOutgoingRelationships?
    //todo: move generic queries into neo4j
    public class NodeAndOutRelationshipsAndTheirInRelationshipsQuery : IQuery<INodeAndOutRelationshipsAndTheirInRelationships?>
    {
        private IEnumerable<string> NodeLabels { get; }
        private List<ContentItemRelationship>? Relationships { get; set; }
        private string IdPropertyName { get; }
        private object IdPropertyValue { get; }

        private List<string>? NodeAliases { get; set; }
        private List<string>? NodeRelationships { get; set; }

        public NodeAndOutRelationshipsAndTheirInRelationshipsQuery(
            IEnumerable<string> nodeLabels,
            string idPropertyName,
            object idPropertyValue,
            List<ContentItemRelationship>? relationships)
        {
            NodeLabels = nodeLabels;
            IdPropertyName = idPropertyName;
            IdPropertyValue = idPropertyValue;
            Relationships = relationships;
        }

        public List<string> ValidationErrors()
        {
            var validationErrors = new List<string>();

            if (!NodeLabels.Any())
            {
                validationErrors.Add("At least one NodeLabel must be provided.");
            }

            //todo: needs to validate id property name and value too

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();

                // irn might have a different IdPropertyName, but if it has, it isn't the source node, so the where is ok
                if (Relationships == null || Relationships.Count == 0)
                {
                    return new Query(
                        $@"match (s:{string.Join(":", NodeLabels)} {{{IdPropertyName}: '{IdPropertyValue}'}})
optional match (s)-[r]->(d)
optional match (d)<-[ir]-(irn) where irn.{IdPropertyName} <> s.{IdPropertyName}
with s, {{destNode: d, relationship: r, destinationIncomingRelationships:collect({{destIncomingRelationship:ir,  destIncomingRelSource:irn}})}} as relationshipDetails
with {{sourceNode: s, outgoingRelationships: collect(relationshipDetails)}} as nodeAndOutRelationshipsAndTheirInRelationships
return nodeAndOutRelationshipsAndTheirInRelationships");
                }
                else
                {
                    var relationships = BuildRelationships();
                    return new Query(relationships);
                }
            }
        }

        private static string IntToLetters(int value)
        {
            string result = string.Empty;
            while (--value >= 0)
            {
                result = (char)('A' + value % 26) + result;
                value /= 26;
            }
            return result;
        }

        private readonly Dictionary<List<string>, string> _aliasLookup = new Dictionary<List<string>, string>();

        private string BuildRelationships()
        {
            int relationshipNumber = 1;
            NodeAliases = new List<string>();
            NodeRelationships = new List<string>();

            StringBuilder sb = new StringBuilder();
            StringBuilder with = new StringBuilder();
            List<string> withAliases = new List<string>();

            sb.AppendLine($@"match (A:{string.Join(":", NodeLabels)} {{{IdPropertyName}: '{IdPropertyValue}'}})");

            foreach (var relationship in Relationships!)
            {
                var sourceNodePrefix = _aliasLookup.FirstOrDefault(x => x.Key.Any(z => z == relationship.Source)).Value;

                if (sourceNodePrefix == null)
                {
                    sourceNodePrefix = IntToLetters(relationshipNumber);
                    relationshipNumber++;
                    NodeAliases.Add(sourceNodePrefix);
                    _aliasLookup.Add(new List<string> { relationship.Source! }, sourceNodePrefix);
                }

                var destinationNodePrefix = _aliasLookup.FirstOrDefault(x => x.Key == relationship.Destination).Value;

                if(destinationNodePrefix == null)
                {
                    destinationNodePrefix = IntToLetters(relationshipNumber);
                    relationshipNumber++;
                    NodeAliases.Add(destinationNodePrefix);
                    _aliasLookup.Add(relationship.Destination.ToList(), destinationNodePrefix);
                }

                var relationshipName = $"{sourceNodePrefix}{destinationNodePrefix}";

                var withAlias = $"{sourceNodePrefix}{destinationNodePrefix}RelationshipDetails";

                NodeRelationships.Add($"{sourceNodePrefix}{destinationNodePrefix}");

                with.AppendLine($"with <<ReplaceAliases>>,<<ReplaceRelationships>>,{$"{string.Join(',', withAliases)}"}{$"{(withAliases.Any() ? "," : "")}"} {{destNode: {destinationNodePrefix}, relationship: {relationshipName}, destinationIncomingRelationships:collect({{destIncomingRelationship:{relationshipName},  destIncomingRelSource:'todo'}})}} as {withAlias}");
                sb.AppendLine($"optional match ({sourceNodePrefix}:{relationship.Source})-[{relationshipName}:{relationship.Relationship}]->({destinationNodePrefix}:{string.Join(":", relationship.Destination!)})");
                withAliases.Add(withAlias);
            }

            with.AppendLine($"with {{sourceNode: A, outgoingRelationships: {BuildCollects(withAliases)}}} as nodeAndOutRelationshipsAndTheirInRelationships");
            var withs = with.ToString();
            withs = withs.Replace("<<ReplaceAliases>>", string.Join(',', NodeAliases));
            withs = withs.Replace("<<ReplaceRelationships>>", string.Join(',', NodeRelationships));

            Console.WriteLine(withs);

            sb.AppendLine(withs);
            sb.AppendLine($"return nodeAndOutRelationshipsAndTheirInRelationships");

            return sb.ToString();
        }

        private string BuildCollects(List<string> withAliases)
        {
            StringBuilder collectString = new StringBuilder();
            foreach(var alias in withAliases)
            {
                collectString.Append($"collect({alias}) + ");
            }

            return collectString.ToString().TrimEnd(' ').Trim('+');
        }

        public INodeAndOutRelationshipsAndTheirInRelationships? ProcessRecord(IRecord record)
        {
            var results = (Dictionary<string, object>)record["nodeAndOutRelationshipsAndTheirInRelationships"];
            if (results == null)
                throw new QueryResultException($"{nameof(NodeAndOutRelationshipsAndTheirInRelationshipsQuery)} results not in expected format.");

            if (!(results["sourceNode"] is INode sourceNode))
                return null;

            //todo: return as IOutgoingRelationships, rather than tuple then convert

            IEnumerable<(IRelationship, INode, IEnumerable<(IRelationship, INode)>)> outgoingRelationships =
                ((IEnumerable<object>)results["outgoingRelationships"])
                .Cast<IDictionary<string, object>>()
                .Select(or =>
                    ((IRelationship)or["relationship"],
                        (INode)or["destNode"],
                        ((IEnumerable<object>)or["destinationIncomingRelationships"])
                        .Cast<IDictionary<string, object>>()
                        .Select(ir =>
                            ((IRelationship)ir["destIncomingRelationship"],
                                (INode)ir["destIncomingRelSource"]))
                        .Where(t => t.Item1 != null)))
                .Where(t => t.Item1 != null);

            return new NodeAndOutRelationshipsAndTheirInRelationships(sourceNode, outgoingRelationships);
        }
    }
}

