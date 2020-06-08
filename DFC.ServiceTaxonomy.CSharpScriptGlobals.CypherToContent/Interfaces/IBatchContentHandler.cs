using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces
{
    public interface IBatchContentHandler
    {
        Task Save(IEnumerable<ContentItem> contentItems);
    }
}
