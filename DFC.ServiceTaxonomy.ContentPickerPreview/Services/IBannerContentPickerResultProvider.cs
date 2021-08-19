using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentPickerPreview.Services
{
    public interface IBannerContentPickerResultProvider
    {
        string Name { get; }

        Task<IEnumerable<BannerContentPickerResult>> Search(ContentPickerSearchContext searchContext);
    }
}
