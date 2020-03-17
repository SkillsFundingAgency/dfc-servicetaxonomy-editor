using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class GraphSyncPartGraphSyncer : IContentPartGraphSyncer
    {
        public string? PartName => nameof(GraphSyncPart);

        //todo: pass IGraphSyncHelper instead of GraphSyncPartSettings
        public Task<IEnumerable<ICommand>> AddSyncComponents(
            dynamic graphSyncContent,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            mergeNodeCommand.Properties.Add(graphSyncHelper.IdPropertyName, graphSyncHelper.GetIdPropertyValue(graphSyncContent));

            return Task.FromResult(Enumerable.Empty<ICommand>());
        }
    }
}
