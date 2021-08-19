using System.Collections.Generic;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Models.Parts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public class BasicPropertyService : IPropertyService
    {
        public bool CanProcess(JToken? jToken, string? propertyName = null)
        {
            return jToken != null && jToken.Type != JTokenType.Object && jToken.Type != JTokenType.Array;
        }

        public IList<PropertyDto> Process(string propertyName, JToken? jToken)
        {
            var objectValue = new ObjectValue { Value = jToken?.ToString() };
            return new List<PropertyDto> { new PropertyDto { Name = propertyName, Value = objectValue.ToString() }};
        }
    }
}
