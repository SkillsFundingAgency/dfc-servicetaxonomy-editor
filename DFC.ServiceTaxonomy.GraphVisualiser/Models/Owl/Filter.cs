using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl
{
    public partial class Filter
    {
        public List<CheckBox> CheckBox { get; set; } = new List<CheckBox>();
        public string? DegreeSliderValue { get; set; }
    }
}
