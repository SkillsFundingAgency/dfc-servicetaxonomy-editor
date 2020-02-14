using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class BagPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IServiceProvider _serviceProvider;

        // private readonly IGraphSyncer _graphSyncer;
        public string? PartName => nameof(BagPart);

        public BagPartGraphSyncer(IServiceProvider serviceProvider) // todo: circular dependency IGraphSyncer graphSyncer)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task AddSyncComponents(
            dynamic graphLookupContent,
            IDictionary<string, object> nodeProperties,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>> nodeRelationships,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            foreach (JObject? contentItem in graphLookupContent.ContentItems)
            {
                var _graphSyncer = _serviceProvider.GetService<IGraphSyncer>();

                await _graphSyncer.SyncToGraph(contentItem!["ContentType"]!.ToString(), contentItem!);
            }
        }
    }
}
