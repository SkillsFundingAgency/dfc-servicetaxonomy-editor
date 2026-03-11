using System.Collections.Generic;
using System.Text.Json;
using DFC.ServiceTaxonomy.VersionComparison.Models;

namespace DFC.ServiceTaxonomy.VersionComparison.Services.PropertyServices
{
    public interface IPropertyService
    {
        bool CanProcess(JsonElement? jElement, string? propertyName = null);

        IList<PropertyExtract> Process(string propertyName, JsonElement? jElement);
    }
}
