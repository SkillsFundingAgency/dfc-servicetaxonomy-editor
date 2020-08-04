using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands.Interfaces
{
    public interface IMergeContentItemNodeCommand
    {
        IContentItemVersion? ContentItemVersion { get; set; }
    }
}
