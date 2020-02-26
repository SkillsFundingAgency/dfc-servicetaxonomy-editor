using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class GraphSyncPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IGraphSyncPartIdProperty _graphSyncPartIdProperty;

        public GraphSyncPartGraphSyncer(IGraphSyncPartIdProperty graphSyncPartIdProperty)
        {
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
        }

        public string? PartName => nameof(GraphSyncPart);

        public Task<IEnumerable<Query>> AddSyncComponents(
            dynamic graphSyncContent,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            mergeNodeCommand.Properties.Add(_graphSyncPartIdProperty.Name, _graphSyncPartIdProperty.Value(graphSyncContent));

            return Task.FromResult(Enumerable.Empty<Query>());
        }
    }
}
