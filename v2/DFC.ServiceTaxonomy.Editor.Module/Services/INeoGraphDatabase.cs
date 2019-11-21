using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Editor.Module.Services
{
    public interface INeoGraphDatabase
    {
        //Task Merge(string nodeLabel, string propertyName, object propertyValue);
        Task Merge(string nodeLabel, IEnumerable<(string Name, string Value)> properties);
    }
}