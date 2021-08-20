using System.Collections.Generic;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public class NullPropertyService : IPropertyService
    {
        public bool CanProcess(JToken? jToken, string? propertyName = null)
        {
            return jToken == null;
        }

        public IList<PropertyExtract> Process(string propertyName, JToken? jToken)
        {
            return new List<PropertyExtract> { new PropertyExtract{ Name = propertyName, Key = propertyName} };
        }
    }
}
