using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class GraphSyncPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IGraphSyncHelper _graphSyncHelper;

        public GraphSyncPartGraphSyncer(IGraphSyncHelper graphSyncHelper)
        {
            _graphSyncHelper = graphSyncHelper;
        }

        public string? PartName => nameof(GraphSyncPart);

        //todo: pass IGraphSyncHelper instead of GraphSyncPartSettings
        public Task<IEnumerable<ICommand>> AddSyncComponents(
            dynamic graphSyncContent,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            GraphSyncPartSettings graphSyncPartSettings)
        {
            mergeNodeCommand.Properties.Add(_graphSyncHelper.IdPropertyName, _graphSyncHelper.IdPropertyValue(graphSyncContent));

            return Task.FromResult(Enumerable.Empty<ICommand>());
        }
    }
}
