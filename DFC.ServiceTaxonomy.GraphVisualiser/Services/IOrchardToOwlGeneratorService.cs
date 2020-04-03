using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Services
{
    public interface IOrchardToOwlGeneratorService
    {
        OwlDataModel? CreateOwlDataModels(IEnumerable<ContentTypeDefinition> contentTypeDefinitions);
    }
}
