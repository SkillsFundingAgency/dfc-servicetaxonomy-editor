namespace DFC.ServiceTaxonomy.GraphVisualiser.Models.Owl
{
    public partial class Settings
    {
        public Global Global { get; set; } = new Global();
        public Gravity Gravity { get; set; } = new Gravity();
        public Filter Filter { get; set; } = new Filter();
        public Options Options { get; set; } = new Options();
        public Modes Modes { get; set; } = new Modes();
    }
}
