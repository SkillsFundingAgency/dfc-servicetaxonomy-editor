using System.Collections.Generic;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public interface IPropertyService
    {
        bool CanProcess(JToken? jToken, string? propertyName = null);

        IList<PropertyExtract> Process(string propertyName, JToken? jToken);
    }
}
