using DFC.ServiceTaxonomy.GraphSync.Models;
using GraphQL.Types;

namespace DFC.ServiceTaxonomy.GraphSync.GraphQL
{
    public class GraphSyncPartQueryObjectType : ObjectGraphType<GraphSyncPart>
    {
        public GraphSyncPartQueryObjectType()
        {
            Name = $"{nameof(GraphSyncPart)}";

            Field("NodeId", x => x.Text, nullable: true);
        }
    }
}
