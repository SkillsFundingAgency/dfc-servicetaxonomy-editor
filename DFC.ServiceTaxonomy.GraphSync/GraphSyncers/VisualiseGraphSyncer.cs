using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Helper;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class VisualiseGraphSyncer : IVisualiseGraphSyncer
    {
        private readonly IContentManager _contentManager;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IDescribeContentItemHelper _describeContentItemHelper;
        private readonly IGraphCluster _neoGraphCluster;
        private readonly IServiceProvider _serviceProvider;

        private static readonly List<string> s_cosmosPropsToIgnore = new List<string>
        {
            "_rid",
            "_self",
            "_etag",
            "_attachments",
            "_ts",
            "_links"
        };

        private static readonly List<string> s_typesToNotLookDeeperAt = new List<string>
        {
            "taxonomy"
        };

        public VisualiseGraphSyncer(
            IContentManager contentManager,
            ISyncNameProvider syncNameProvider,
            IDescribeContentItemHelper describeContentItemHelper,
            IGraphCluster neoGraphCluster,
            IServiceProvider serviceProvider)
        {
            _contentManager = contentManager;
            _syncNameProvider = syncNameProvider;
            _describeContentItemHelper = describeContentItemHelper;
            _neoGraphCluster = neoGraphCluster;
            _serviceProvider = serviceProvider;
        }

        //todo: if issue with data, don't just hang visualiser
        private async Task<IEnumerable<IQuery<object?>>> BuildVisualisationCommands(
            string contentItemId,
            IContentItemVersion contentItemVersion)
        {
            ContentItem? contentItem = await contentItemVersion.GetContentItem(_contentManager, contentItemId);
            if (contentItem == null)
            {
                return Enumerable.Empty<IQuery<INodeAndOutRelationshipsAndTheirInRelationships>>();
            }
            //todo: best to not use dynamic
            dynamic? graphSyncPartContent = contentItem.Content[nameof(GraphSyncPart)];

            _syncNameProvider.ContentType = contentItem.ContentType;

            string? sourceNodeId = _syncNameProvider.GetNodeIdPropertyValue(graphSyncPartContent, contentItemVersion);
            IEnumerable<string> sourceNodeLabels = await _syncNameProvider.NodeLabels();
            string sourceNodeIdPropertyName = _syncNameProvider.IdPropertyName();

            var rootContext = await _describeContentItemHelper.BuildRelationships(
                contentItem, sourceNodeIdPropertyName, sourceNodeId, sourceNodeLabels, _syncNameProvider,
                _contentManager, contentItemVersion, null, _serviceProvider);

            //todo: return relationships - can we do it without creating cypher outside of a query?

            if (rootContext == null)
                return Enumerable.Empty<IQuery<object?>>();

            //todo: should create relationships in here
            return await _describeContentItemHelper.GetRelationshipCommands(rootContext);
        }

        public async Task<Subgraph> GetVisualisationSubgraph(
            string contentItemId,
            string graphName,
            IContentItemVersion contentItemVersion)
        {
            var relationshipCommands = await BuildVisualisationCommands(contentItemId, contentItemVersion!);

            // Get all results atomically
            var queryResults = await _neoGraphCluster.Run(graphName, relationshipCommands.ToArray());
            var outgoingResults = queryResults
                .Select(queryResult => (queryResult as JObject)?.ToObject<INodeAndOutRelationshipsAndTheirInRelationships>())
                .ToList();

            await AddExtraDetailForOutgoingLinks(outgoingResults.First(), graphName);

            //todo: should really always return the source node (until then, the subgraph will pull it if the main results don't)
            Subgraph subgraph = new Subgraph();

            if (outgoingResults.Any())
            {
                var nodes = outgoingResults
                    .SelectMany(outgoingResult =>
                        outgoingResult!.OutgoingRelationships
                            .Select(outgoingRelationship =>
                                outgoingRelationship.outgoingRelationship.DestinationNode)
                            .GroupBy(outgoingRelationship => outgoingRelationship.Id)
                            .Select(orGroup => orGroup.First()))
                    .Union(outgoingResults
                        .GroupBy(outgoingResult => outgoingResult!.SourceNode)
                        .Select(orGroup => orGroup.First()!.SourceNode));

                var relationships = outgoingResults!
                    .SelectMany(outgoingResult => outgoingResult!.OutgoingRelationships
                        .Select(orRelationship => orRelationship.outgoingRelationship.Relationship))
                    .ToHashSet();

                // Get all outgoing relationships from the query and add in any source nodes
                subgraph = new Subgraph(
                    nodes,
                    relationships,
                    outgoingResults.FirstOrDefault()?.SourceNode);
            }

            ISubgraph? incomingResults = queryResults
                .Select(queryResult => (queryResult as JObject)?.ToObject<Subgraph>())
                .FirstOrDefault(incomingResult => incomingResult != null);

            if (incomingResults != null)
            {
                await AddExtraDetailForIncomingLinks(incomingResults, graphName, subgraph.SourceNode!);
                subgraph.Add(incomingResults);
            }

            foreach (var subgraphNode in subgraph.Nodes)
            {
                subgraphNode.Properties = StripUndesiredProperties(subgraphNode.Properties);
            }

            return subgraph;
        }

        private static Dictionary<string, object> StripUndesiredProperties(IReadOnlyDictionary<string, object> record)
        {
            var returnProperties = new Dictionary<string, object>();

            foreach (var propertyName in record.Keys)
            {
                if (s_cosmosPropsToIgnore.Contains(propertyName))
                {
                    continue;
                }

                var propertyValue = record[propertyName];
                returnProperties.Add(propertyName, propertyValue);
            }

            return returnProperties;
        }

        private async Task AddExtraDetailForIncomingLinks(ISubgraph? incomingResults, string graphName, INode sourceNode)
        {
            var nodes = incomingResults!.Nodes.ToList();
            var nodesIncludingSourceNode = nodes.Union(new List<INode> { sourceNode }).ToList();

            foreach (var contentType in GetDistinctContentTypes(nodes))
            {
                var detailResults = await GetLinksDetail(nodes, contentType, graphName);

                foreach (var detailResult in detailResults)
                {
                    var properties = (detailResult as JObject)!.ToObject<Dictionary<string, object>>();
                    var itemId = GetAsString(properties!["id"]);

                    var destinationNode = nodes.Single(node => GetAsString(node.Properties["id"]) == itemId);
                    destinationNode.Properties = properties;

                    var relationship = incomingResults.Relationships
                        .Single(rel => rel.StartNodeId == destinationNode.Id);
                    var otherNode = nodesIncludingSourceNode.Single(node => node.Id == relationship.EndNodeId);
                    var otherId = GetAsString(otherNode.Properties["id"]);

                    relationship.Type = GetContHasName(properties, otherId, relationship.Type);
                }
            }
        }

        private static string GetContHasName(Dictionary<string, object> properties, string id, string defaultName)
        {
            var links = (properties["_links"] as JObject)?.ToObject<Dictionary<string, object>>();

            if (links == null)
            {
                throw new MissingFieldException("Links property is missing");
            }

            foreach (var link in links.Where(x => x.Key != "self" && x.Key != "curies"))
            {
                if (link.Value is JArray jArray)
                {
                    var linkListDictionary = jArray.ToObject<List<Dictionary<string, object>>>();

                    foreach (var linkDictionary2 in linkListDictionary!)
                    {
                        var href2 = (string)linkDictionary2["href"];
                        (string _, Guid id2) = GetContentTypeAndId(href2);

                        if (id2.ToString() != id)
                        {
                            continue;
                        }

                        return link.Key.Replace("cont:", string.Empty);
                    }

                    continue;
                }

                var linkDictionary = (link.Value as JObject)!.ToObject<Dictionary<string, object>>();
                var href3 = (string)linkDictionary!["href"];
                (string _, Guid id3) = GetContentTypeAndId(href3);

                if (id3.ToString() != id)
                {
                    continue;
                }

                return link.Key.Replace("cont:", string.Empty);
            }

            return defaultName;
        }

        private async Task AddExtraDetailForOutgoingLinks(INodeAndOutRelationshipsAndTheirInRelationships? outgoingResult, string graphName)
        {
            var relationships = outgoingResult!.OutgoingRelationships
                .Select(outgoingRelationships => outgoingRelationships.outgoingRelationship)
                .ToList();

            var destinationNodes = outgoingResult!.OutgoingRelationships
                .Select(outgoingRelationships => outgoingRelationships.outgoingRelationship.DestinationNode)
                .ToList();

            var outgoingRelationships = outgoingResult.OutgoingRelationships.ToList();

            foreach (var contentType in GetDistinctContentTypes(destinationNodes))
            {
                var detailResults = await GetLinksDetail(destinationNodes, contentType, graphName);

                foreach (var detailResult in detailResults)
                {
                    var properties = ((detailResult as JObject)!).ToObject<Dictionary<string, object>>();
                    var itemId = GetAsString(properties!["id"]);

                    var itemRelationship = relationships
                        .Single(relationship => GetAsString(relationship.DestinationNode.Properties["id"]) == itemId);
                    var destinationNode = itemRelationship.DestinationNode;

                    var endNodeId = (int)destinationNode.Properties["endNodeId"];
                    destinationNode.Properties = properties;

                    var futherOutgoingLinks =
                        await Get2ndLevelOutgoingLinks(properties, graphName, endNodeId);

                    foreach (var futherOutgoingLink in futherOutgoingLinks)
                    {
                        outgoingRelationships.Add(futherOutgoingLink);
                    }
                }
            }

            outgoingResult.OutgoingRelationships = outgoingRelationships;
        }

        private async Task<List<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)>> Get2ndLevelOutgoingLinks(
            Dictionary<string, object> properties,
            string graphName,
            int startNodeId)
        {
            var returnList = new List<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)>();
            var parentId = GetAsString(properties["id"]);
            var parentContentType = (string)properties["ContentType"];

            if (s_typesToNotLookDeeperAt.Contains(parentContentType))
            {
                return returnList;
            }

            var links = (properties["_links"] as JObject)?.ToObject<Dictionary<string, object>>();

            if (links == null)
            {
                return returnList;
            }

            var contentTypesWithIds = new Dictionary<string, List<Guid>>();

            foreach (var link in links.Where(x => x.Key != "self" && x.Key != "curies"))
            {
                if (link.Value is JArray jArray)
                {
                    var linkListDictionary = jArray.ToObject<List<Dictionary<string, object>>>();

                    foreach (var linkDictionary2 in linkListDictionary!)
                    {
                        var href2 = (string)linkDictionary2["href"];
                        (string contentType2, Guid id2) = GetContentTypeAndId(href2);

                        var key2 = $"{contentType2}|{link.Key}";

                        if (!contentTypesWithIds.ContainsKey(key2))
                        {
                            contentTypesWithIds.Add(key2, new List<Guid>());
                        }

                        contentTypesWithIds[key2].Add(id2);
                    }

                    continue;
                }

                var linkDictionary = (link.Value as JObject)!.ToObject<Dictionary<string, object>>();
                var href = (string)linkDictionary!["href"];
                (string contentType, Guid id) = GetContentTypeAndId(href);

                var key = $"{contentType}|{link.Key}";

                if (!contentTypesWithIds.ContainsKey(key))
                {
                    contentTypesWithIds.Add(key, new List<Guid>());
                }

                contentTypesWithIds[key].Add(id);
            }

            foreach (var contentTypeWithIds in contentTypesWithIds)
            {
                var ids = contentTypeWithIds
                    .Value
                    .Select(id => $"'{GetAsString(id)}'")
                    .ToArray();

                var contentType = contentTypeWithIds.Key.Split('|')[0];
                var detailResults = await RunQuery(ids, contentType, graphName);

                foreach (var detailResult in detailResults)
                {
                    var properties2 = ((detailResult as JObject)!).ToObject<Dictionary<string, object>>();
                    var p2 = GetContentTypeAndId((string)properties2!["uri"]);

                    var endNodeId = UniqueSequencedNumber.GetNumber(GetAsString(p2.Id));
                    var relationshipId = UniqueSequencedNumber.GetNumber(GetAsString(p2.Id) + parentId);

                    var outgoing = new OutgoingRelationship(new StandardRelationship
                    {
                        Type = contentTypeWithIds.Key.Split('|')[1].Replace("cont:", string.Empty), // e.g. cont:hasPageLocation
                        StartNodeId = startNodeId,
                        EndNodeId = endNodeId,
                        Id = relationshipId
                    }, new StandardNode
                    {
                        Id = endNodeId,
                        Properties = properties2,
                        Labels = new List<string> { p2.ContentType, "Resource" }
                    });

                    returnList.AddRange(new List<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)>
                    {
                        (outgoing, new List<IOutgoingRelationship>())
                    });
                }
            }

            return returnList;
        }

        private Task<List<object?>> GetLinksDetail(List<INode> destinationNodes, string contentType, string graphName)
        {
            var ids = destinationNodes
                .Where(node => contentType.Equals((string)node.Properties["ContentType"], StringComparison.InvariantCultureIgnoreCase))
                .Select(node => $"'{GetAsString(node.Properties["id"])}'")
                .ToArray();

            return RunQuery(ids, contentType, graphName);
        }

        private Task<List<object?>> RunQuery(string[] ids, string contentType, string graphName)
        {
            var detailCommand = new List<IQuery<object?>>
            {
                new CosmosDbNodeAndNestedOutgoingRelationshipsQuery(
                    $"select * from c where c.id in ({string.Join(',', ids)})|{contentType}")
            }.ToArray();

            return _neoGraphCluster.Run(graphName, detailCommand);
        }

        protected static (string ContentType, Guid Id) GetContentTypeAndId(string uri)
        {
            var pathOnly = uri.StartsWith("http") ? new Uri(uri, UriKind.Absolute).AbsolutePath : uri;
            pathOnly = pathOnly.ToLower().Replace("/api/execute", string.Empty);

            var uriParts = pathOnly.Trim('/').Split('/');
            var contentType = uriParts[0].ToLower();
            var id = Guid.Parse(uriParts[1]);

            return (contentType, id);
        }

        private static string GetAsString(object item)
        {
            if (item is Guid guidItem)
            {
                return guidItem.ToString();
            }

            if (item is JValue jValueItem)
            {
                return jValueItem.ToString(CultureInfo.InvariantCulture);
            }

            return (string)item;
        }

        private static List<string> GetDistinctContentTypes(List<INode> destinationNodes)
        {
            var distinctContentTypes = new List<string>();

            foreach (var destinationNode in destinationNodes)
            {
                var contentType = (string)destinationNode.Properties["ContentType"];

                if (!distinctContentTypes.Contains(contentType))
                {
                    distinctContentTypes.Add(contentType);
                }
            }

            return distinctContentTypes;
        }
    }
}
