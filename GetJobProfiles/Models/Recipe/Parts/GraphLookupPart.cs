namespace GetJobProfiles.Models.Recipe.Parts
{
    public class GraphLookupPart
    {
        public Node[] Nodes { get; set; }
    }

    public class Node
    {
        public string Id { get; set; }
        public string DisplayText { get; set; }
    }
}
