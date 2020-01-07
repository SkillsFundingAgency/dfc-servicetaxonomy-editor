using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Models;
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

        public Task AddSyncComponents(
            dynamic graphSyncContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            nodeProperties.Add(_graphSyncPartIdProperty.Name, _graphSyncPartIdProperty.Value(graphSyncContent));
            return Task.CompletedTask;
        }
    }
}
