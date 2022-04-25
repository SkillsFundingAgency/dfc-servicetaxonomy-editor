using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Helpers;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.GraphSync.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using static DFC.ServiceTaxonomy.GraphSync.Helpers.DocumentHelper;
using static DFC.ServiceTaxonomy.GraphSync.Helpers.UniqueNumberHelper;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class VisualiseGraphSyncer : IVisualiseGraphSyncer
    {
        private readonly IContentManager _contentManager;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IDescribeContentItemHelper _describeContentItemHelper;
        private readonly IGraphCluster _neoGraphCluster;
        private readonly IServiceProvider _serviceProvider;

        private static readonly List<string> s_relationshipsToIgnore = new List<string>
        {
            "page>taxonomy>pagelocation",
            "page>pagelocation>pagelocation"
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
            var relationshipCommands = await BuildVisualisationCommands(contentItemId, contentItemVersion);

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

                var relationships = outgoingResults
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

            foreach (string propertyName in record.Keys)
            {
                if (DocumentHelper.CosmosPropsToIgnore.Contains(propertyName))
                {
                    continue;
                }

                object propertyValue = record[propertyName];
                returnProperties.Add(propertyName, propertyValue);
            }

            return returnProperties;
        }

        private async Task AddExtraDetailForIncomingLinks(ISubgraph? incomingResults, string graphName, INode sourceNode)
        {
            var nodes = incomingResults!.Nodes.ToList();
            var nodesIncludingSourceNode = nodes.Union(new List<INode> { sourceNode }).ToList();

            foreach (string contentType in GetDistinctContentTypes(nodes))
            {
                var detailResults = await RetrieveLinksDetail(nodes, contentType, graphName);

                foreach (object? detailResult in detailResults)
                {
                    var properties = SafeCastToDictionary(detailResult);
                    string itemId = GetAsString(properties!["id"]);

                    var destinationNode = nodes.Single(node => GetAsString(node.Properties["id"]) == itemId);
                    destinationNode.Properties = properties;

                    var relationship = incomingResults.Relationships
                        .Single(rel => rel.StartNodeId == destinationNode.Id);
                    var otherNode = nodesIncludingSourceNode.Single(node => node.Id == relationship.EndNodeId);
                    string otherId = GetAsString(otherNode.Properties["id"]);

                    relationship.Type = GetContHasName(properties, otherId, relationship.Type);
                }
            }
        }

        private static string GetContHasName(Dictionary<string, object> properties, string parentId, string defaultName)
        {
            var links = SafeCastToDictionary(properties["_links"]);

            if (links == null)
            {
                throw new MissingFieldException("Links property is missing");
            }

            foreach (var link in links.Where(lnk => lnk.Key != "self" && lnk.Key != "curies"))
            {
                if (link.Value is JArray jArray)
                {
                    string? returnItem = GetContHasNameForArrayItem(jArray, link, parentId);
                    if (returnItem == null)
                    {
                        continue;
                    }

                    return returnItem;
                }

                var linkDictionary = SafeCastToDictionary(link.Value);
                string linkHref = (string)linkDictionary!["href"];
                var (_, linkId) = GetContentTypeAndId(linkHref);

                if (linkId.ToString() != parentId)
                {
                    continue;
                }

                return link.Key.Replace("cont:", string.Empty);
            }

            return defaultName;
        }

        private static string? GetContHasNameForArrayItem(JArray jArray, KeyValuePair<string, object> link, string parentId)
        {
            var linkListDictionary = jArray.ToObject<List<Dictionary<string, object>>>();

            foreach (var linkDictionary in linkListDictionary!)
            {
                string linkHref = (string)linkDictionary["href"];
                var (_, linkId) = GetContentTypeAndId(linkHref);

                if (linkId.ToString() != parentId)
                {
                    continue;
                }

                return link.Key.Replace("cont:", string.Empty);
            }

            return null;
        }

        private async Task AddExtraDetailForOutgoingLinks(
            INodeAndOutRelationshipsAndTheirInRelationships? outgoingResult,
            string graphName)
        {
            var relationships = outgoingResult!.OutgoingRelationships
                .Select(outgoingRelationships => outgoingRelationships.outgoingRelationship)
                .ToList();

            var destinationNodes = outgoingResult.OutgoingRelationships
                .Select(outgoingRelationships => outgoingRelationships.outgoingRelationship.DestinationNode)
                .ToList();

            string rootContentType = (string)outgoingResult.SourceNode.Properties["ContentType"];
            var outgoingRelationships = outgoingResult.OutgoingRelationships.ToList();

            foreach (string contentType in GetDistinctContentTypes(destinationNodes))
            {
                var detailResults = await RetrieveLinksDetail(destinationNodes, contentType, graphName);

                foreach (object? detailResult in detailResults)
                {
                    var properties = SafeCastToDictionary(detailResult);
                    string itemId = GetAsString(properties!["id"]);

                    var itemRelationship = relationships
                        .Single(relationship => GetAsString(relationship.DestinationNode.Properties["id"]) == itemId);
                    var destinationNode = itemRelationship.DestinationNode;

                    int endNodeId = (int)destinationNode.Properties["endNodeId"];
                    destinationNode.Properties = properties;

                    var furtherOutgoingLinks =
                        await GetSecondLevelOutgoingLinks(properties, graphName, endNodeId, rootContentType);

                    foreach (var furtherOutgoingLink in furtherOutgoingLinks)
                    {
                        outgoingRelationships.Add(furtherOutgoingLink);
                    }
                }
            }

            outgoingResult.OutgoingRelationships = outgoingRelationships;
        }

        private async Task<List<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)>>
            GetSecondLevelOutgoingLinks(
                Dictionary<string, object> properties,
                string graphName,
                int startNodeId,
                string rootContentType)
        {
            var returnList = new List<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)>();
            string parentId = GetAsString(properties["id"]);
            string parentContentType = (string)properties["ContentType"];

            var links = SafeCastToDictionary(properties["_links"]);
            var contentTypesWithIds = new Dictionary<string, List<Guid>>();

            foreach (var link in links.Where(lnk => lnk.Key != "self" && lnk.Key != "curies"))
            {
                if (link.Value is JArray jArray)
                {
                    GetSecondLevelOutgoingLinksForArray(jArray, contentTypesWithIds, rootContentType, parentContentType, link);
                    continue;
                }

                var linkDictionary = SafeCastToDictionary(link.Value);
                string href = (string)linkDictionary["href"];
                (string contentType, Guid id) = GetContentTypeAndId(href);

                if (s_relationshipsToIgnore.Contains($"{rootContentType}>{parentContentType}>{contentType}"))
                {
                    continue;
                }

                string key = $"{contentType}|{link.Key}";

                if (!contentTypesWithIds.ContainsKey(key))
                {
                    contentTypesWithIds.Add(key, new List<Guid>());
                }

                contentTypesWithIds[key].Add(id);
            }

            foreach (var contentTypeWithIds in contentTypesWithIds)
            {
                string[] ids = contentTypeWithIds
                    .Value
                    .Select(id => $"'{GetAsString(id)}'")
                    .ToArray();

                string contentType = contentTypeWithIds.Key.Split('|')[0];
                List<object?> detailResults = await RunQuery(ids, contentType, graphName);

                foreach (object? detailResult in detailResults)
                {
                    Dictionary<string, object>? detailResultProperties = SafeCastToDictionary(detailResult);
                    (string detailResultContentType, Guid detailResultId) = GetContentTypeAndId((string)detailResultProperties!["uri"]);

                    int endNodeId = GetNumber(GetAsString(detailResultId));
                    int relationshipId = GetNumber(GetAsString(detailResultId) + parentId);

                    var outgoing = new OutgoingRelationship(new StandardRelationship
                    {
                        Type = contentTypeWithIds.Key.Split('|')[1].Replace("cont:", string.Empty), // e.g. cont:hasPageLocation
                        StartNodeId = startNodeId,
                        EndNodeId = endNodeId,
                        Id = relationshipId
                    }, new StandardNode
                    {
                        Id = endNodeId,
                        Properties = detailResultProperties,
                        Labels = new List<string> { detailResultContentType, "Resource" }
                    });

                    returnList.AddRange(new List<(IOutgoingRelationship outgoingRelationship, IEnumerable<IOutgoingRelationship> incomingRelationships)>
                    {
                        (outgoing, new List<IOutgoingRelationship>())
                    });
                }
            }

            return returnList;
        }

        private static void GetSecondLevelOutgoingLinksForArray(
            JArray jArray,
            Dictionary<string, List<Guid>> contentTypesWithIds,
            string rootContentType,
            string parentContentType,
            KeyValuePair<string, object> link)
        {
            var linkListDictionary = jArray.ToObject<List<Dictionary<string, object>>>();

            foreach (var linkDictionary in linkListDictionary!)
            {
                string href = (string)linkDictionary["href"];
                (string contentType, Guid id) = GetContentTypeAndId(href);

                if (s_relationshipsToIgnore.Contains($"{rootContentType}>{parentContentType}>{contentType}"))
                {
                    return;
                }

                string key = $"{contentType}|{link.Key}";

                if (!contentTypesWithIds.ContainsKey(key))
                {
                    contentTypesWithIds.Add(key, new List<Guid>());
                }

                contentTypesWithIds[key].Add(id);
            }
        }

        private Task<List<object?>> RetrieveLinksDetail(List<INode> destinationNodes, string contentType, string graphName)
        {
            string[] ids = destinationNodes
                .Where(node => contentType.Equals((string)node.Properties["ContentType"], StringComparison.InvariantCultureIgnoreCase))
                .Select(node => $"'{GetAsString(node.Properties["id"])}'")
                .ToArray();

            return RunQuery(ids, contentType, graphName);
        }

        private Task<List<object?>> RunQuery(string[] ids, string contentType, string graphName)
        {
            var detailCommand = new List<IQuery<object?>>
            {
                new CosmosDbNodeAndNestedOutgoingRelationshipsQuery("SELECT * FROM c WHERE c.id in (@idList0)", "@idList0", string.Join(',', ids), contentType)
            }.ToArray();

            return _neoGraphCluster.Run(graphName, detailCommand);
        }

        private static List<string> GetDistinctContentTypes(List<INode> destinationNodes)
        {
            var distinctContentTypes = new List<string>();

            foreach (var destinationNode in destinationNodes)
            {
                string contentType = (string)destinationNode.Properties["ContentType"];

                if (!distinctContentTypes.Contains(contentType))
                {
                    distinctContentTypes.Add(contentType);
                }
            }

            return distinctContentTypes;
        }
    }
}
