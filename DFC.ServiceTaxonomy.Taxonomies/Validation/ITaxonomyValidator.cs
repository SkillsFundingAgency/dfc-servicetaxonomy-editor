using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.Taxonomies.Validation
{
    public interface ITaxonomyValidator
    {
        Task<TaxonomyValidationResult> Validate(TaxonomyPart part);
    }
}
