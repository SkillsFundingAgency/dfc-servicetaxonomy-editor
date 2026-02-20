using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Models.Parts;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public class SingleChildPropertyService : IPropertyService
    {
        public bool CanProcess(JsonElement? jElement, string? propertyName = null)
        {
            return jElement != null && jElement.GetValueOrDefault().ValueKind == JsonValueKind.Object && jElement.GetValueOrDefault().EnumerateObject().Count() == 1;
        }

        public IList<PropertyExtract> Process(string propertyName, JsonElement? jElement)
        {
            var objectValue = jElement?.Deserialize<ObjectValue>();
            return new List<PropertyExtract> {new PropertyExtract {Key = propertyName, Name = propertyName, Value = objectValue?.ToString()}};
        }
    }
}
