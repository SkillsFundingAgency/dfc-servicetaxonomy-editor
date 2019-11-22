using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Editor.Module.Services
{
    public interface INeoGraphDatabase
    {
        Task Merge(string nodeLabel, IDictionary<string, object> properties);
    }
}