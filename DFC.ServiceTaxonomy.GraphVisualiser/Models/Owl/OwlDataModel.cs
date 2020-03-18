using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl
{
    public partial class OwlDataModel
    {
        public List<Namespace> Namespace { get; set; }
        public Header Header { get; set; }
        public Settings Settings { get; set; }
        public List<Class> Class { get; set; }
        public List<ClassAttribute> ClassAttribute { get; set; }
        public List<Property> Property { get; set; }
        public List<PropertyAttribute> PropertyAttribute { get; set; }
    }
}
