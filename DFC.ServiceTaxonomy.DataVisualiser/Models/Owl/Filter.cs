using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.DataVisualiser.Models.Owl
{
    public partial class Filter
    {
        public List<CheckBox> CheckBox { get; set; } = new List<CheckBox>();
        public string? DegreeSliderValue { get; set; }
    }
}
