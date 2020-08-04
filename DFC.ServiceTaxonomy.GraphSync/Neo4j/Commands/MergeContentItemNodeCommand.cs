using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands
{
    public class MergeContentItemNodeCommand : MergeNodeCommand, IMergeContentItemNodeCommand
    {
        public IContentItemVersion? ContentItemVersion { get; set; }
    }
}
