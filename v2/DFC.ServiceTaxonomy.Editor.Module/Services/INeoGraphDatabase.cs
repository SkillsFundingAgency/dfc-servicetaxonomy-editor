using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Editor.Module.Services
{
    public interface INeoGraphDatabase
    {
        Task MergeNode(string nodeLabel, IDictionary<string,object> propertyMap, string idPropertyName = "uri");
    }
}