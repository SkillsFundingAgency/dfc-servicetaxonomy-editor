using System.Collections.Generic;
using System.Text.Json;
using DFC.ServiceTaxonomy.VersionComparison.Models;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public class NullPropertyService : IPropertyService
    {
        public bool CanProcess(JsonElement? jElement, string? propertyName = null)
        {
            return jElement == null;
        }

        public IList<PropertyExtract> Process(string propertyName, JsonElement? jElement)
        {
            return new List<PropertyExtract> { new PropertyExtract{ Name = propertyName, Key = propertyName} };
        }
    }
}
