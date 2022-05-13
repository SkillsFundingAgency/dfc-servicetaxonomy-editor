using System;
using DFC.ServiceTaxonomy.Neo4j.Configuration;

namespace DFC.ServiceTaxonomy.Neo4j.Services.Interfaces
{
    public interface IGraphClusterBuilder
    {
        IGraphCluster Build(Action<Neo4jOptions>? configure = null);
    }
}
