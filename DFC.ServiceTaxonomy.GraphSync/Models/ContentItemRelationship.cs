using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Models
{
    public class ContentItemRelationship
    {
        public ContentItemRelationship(IEnumerable<string> source, string relationship, IEnumerable<string> destination)
        {
            Source = source;
            Relationship = relationship;
            Destination = destination;
        }

        public IEnumerable<string>? Source { get; set; }
        public string? Relationship { get; set; }
        public IEnumerable<string>? Destination { get; set; }
    }
}
