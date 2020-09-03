using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Taxonomies.Models
{
    public class TaxonomyValidationResult
    {
        public TaxonomyValidationResult(bool valid, List<string> errors)
        {
            Valid = valid;
            Errors = errors;
        }

        public bool Valid { get; }
        public List<string> Errors { get; }
    }
}
