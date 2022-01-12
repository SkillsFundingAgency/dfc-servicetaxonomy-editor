using System.Collections.Generic;
using DFC.ServiceTaxonomy.VersionComparison.Models;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.VersionComparison.Services
{
    public interface IDiffBuilderService
    {
        List<DiffItem> BuildDiffList(ContentItem baseVersion, ContentItem compareVersion);
    }
}
