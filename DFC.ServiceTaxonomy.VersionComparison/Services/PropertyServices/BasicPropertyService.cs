using System.Collections.Generic;
using System.Text.Json;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Models.Parts;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public class BasicPropertyService : IPropertyService
    {
        public bool CanProcess(JsonElement? jElement, string? propertyName = null)
        {
            return jElement != null && jElement.GetValueOrDefault().ValueKind != JsonValueKind.Object && jElement.GetValueOrDefault().ValueKind != JsonValueKind.Array;
        }

        public IList<PropertyExtract> Process(string propertyName, JsonElement? jElement)
        {
            var objectValue = new ObjectValue { Value = jElement?.ToString() };
            return new List<PropertyExtract> { new PropertyExtract { Name = propertyName, Value = objectValue.ToString() }};
        }
    }
}
