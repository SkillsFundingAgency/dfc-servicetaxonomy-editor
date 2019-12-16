namespace DFC.ServiceTaxonomy.GraphLookup.Models
{
    public class GraphLookupNode
    {
        /// <summary>
        /// don't rename - must match code in vue-multiselect-wrapper.js
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// html select value
        /// </summary>
        public string DisplayText { get; }

        public GraphLookupNode(string id, string displayText)
        {
            Id = id;
            DisplayText = displayText;
        }
    }
}
