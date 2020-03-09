using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentPartGraphSyncer
    {
        string? PartName {get;}

        //todo: new type(s) for relationships
        Task<IEnumerable<Query>> AddSyncComponents(
            dynamic graphLookupContent,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition);

        Task<bool> VerifySyncComponent(
            ContentItem contentItem,
            INode node,
            ContentTypePartDefinition contentTypePartDefinition,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destNodes);
    }
}
