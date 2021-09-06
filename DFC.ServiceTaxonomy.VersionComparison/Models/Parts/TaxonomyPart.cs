using System.Collections.Generic;
using System.Linq;

namespace DFC.ServiceTaxonomy.VersionComparison.Models.Parts
{
    public class TaxonomyPart
    {
        public List<TermPart>? Terms { get; set; }

        public string GetDisplayTextById(string contentItemId)
        {
            return GetTerms().FirstOrDefault(t => t.ContentItemId == contentItemId).DisplayText ?? string.Empty;
        }
        private IEnumerable<TermPart> GetTerms(TermPart? term = null)
        {
            var terms = term != null ? term.Terms : Terms;
            if (terms != null)
            {
                foreach (TermPart termPart in terms)
                {
                    yield return termPart;

                    if (termPart.Terms != null)
                    {
                        foreach (TermPart part in GetTerms(termPart))
                        {
                            yield return part;
                        }
                    }
                }
            }
        }
    }
}
