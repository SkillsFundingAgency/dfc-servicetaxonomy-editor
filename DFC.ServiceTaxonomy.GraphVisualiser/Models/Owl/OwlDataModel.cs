using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl
{
    public partial class OwlDataModel
    {
        public List<Namespace> Namespace { get; set; } = new List<Namespace>();
        public Header Header { get; set; } = new Header();
        public Settings Settings { get; set; } = new Settings();
        public List<Class> Class { get; set; } = new List<Class>();
        public List<ClassAttribute> ClassAttribute { get; set; } = new List<ClassAttribute>();
        public List<Property> Property { get; set; } = new List<Property>();
        public List<PropertyAttribute> PropertyAttribute { get; set; } = new List<PropertyAttribute>();
    }
}
