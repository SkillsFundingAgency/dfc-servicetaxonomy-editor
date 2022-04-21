using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataVisualiser.Models.Owl;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataVisualiser.Services
{
    public interface IOrchardToOwlGeneratorService
    {
        OwlDataModel? CreateOwlDataModels(IEnumerable<ContentTypeDefinition> contentTypeDefinitions);
    }
}
