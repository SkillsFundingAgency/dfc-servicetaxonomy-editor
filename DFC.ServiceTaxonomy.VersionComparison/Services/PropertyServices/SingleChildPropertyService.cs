using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using DFC.ServiceTaxonomy.VersionComparison.Models.Parts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public class SingleChildPropertyService : IPropertyService
    {
        public bool CanProcess(JToken? jToken, string? propertyName = null)
        {
            return jToken != null && jToken.Type == JTokenType.Object && jToken.Children().Count() == 1;
        }

        public IList<PropertyDto> Process(string propertyName, JToken? jToken)
        {
            var objectValue = jToken?.ToObject<ObjectValue>();
            return new List<PropertyDto> {new PropertyDto {Key = propertyName, Name = propertyName, Value = objectValue?.ToString()}};
        }
    }
}
