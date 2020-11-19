using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Models
{
    //todo: probably doesn't belong here
    public class ContentItemRelationship
    {
        public ContentItemRelationship(IEnumerable<string> source, string relationship, IEnumerable<string> destination)
        {
            Source = source;
            Relationship = relationship;
            Destination = destination;
        }

        public IEnumerable<string> Source { get; }
        public string Relationship { get; }
        public IEnumerable<string> Destination { get; }
        public string? RelationshipPathString { get; set; }
    }
}
