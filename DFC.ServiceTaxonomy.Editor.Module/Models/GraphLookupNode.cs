namespace DFC.ServiceTaxonomy.Editor.Module.Models
{
    public class GraphLookupNode
    {
        public string Id { get; set; }
        /// <summary>
        /// html select value
        /// </summary>
        public string DisplayText { get; set; }

        public GraphLookupNode(string id, string displayText)
        {
            Id = id;
            DisplayText = displayText;
        }
    }
}
