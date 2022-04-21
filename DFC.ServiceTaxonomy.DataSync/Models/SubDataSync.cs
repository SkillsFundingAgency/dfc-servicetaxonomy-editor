using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.JsonConverters;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.DataSync.Models
{
    [JsonConverter(typeof(SubDataSyncConverter))]
    public class SubDataSync : ISubDataSync
    {
        //todo: immutable, with non-nullable sourcenode?
        public INode? SourceNode { get; set; }
        public HashSet<INode> Nodes { get; }
        public HashSet<IRelationship> Relationships { get; }

        private IRelationship[]? _outgoingRelationships;
        private IRelationship[]? _incomingRelationships;

        public SubDataSync()
        {
            SourceNode = null;
            Nodes = new HashSet<INode>();
            Relationships = new HashSet<IRelationship>();
        }

        public SubDataSync(IEnumerable<INode> nodes, IEnumerable<IRelationship> relationships, INode? sourceNode = null)
        {
            Nodes = new HashSet<INode>(nodes);
            Relationships = new HashSet<IRelationship>(relationships);
            SourceNode = sourceNode;
        }

        public void Add(ISubDataSync subDataSync)
        {
            Nodes.UnionWith(subDataSync.Nodes);
            Relationships.UnionWith(subDataSync.Relationships);

            // options:
            // use SourceNode from new SubDataSync
            // keep original SourceNode
            // validate both same (& throw if not)
            // set to null if different
            SourceNode = subDataSync.SourceNode;
        }

        public IEnumerable<IRelationship> OutgoingRelationships
        {
            get
            {
                if (_outgoingRelationships != null)
                    return _outgoingRelationships;

                //todo: null SourceNode
                return _outgoingRelationships = Relationships
                    .Where(r => r.StartNodeId == SourceNode!.Id)
                    .ToArray();
            }
        }

        public IEnumerable<IRelationship> IncomingRelationships
        {
            get
            {
                if (_incomingRelationships != null)
                    return _incomingRelationships;

                //todo: null SourceNode
                return _incomingRelationships = Relationships
                    .Where(r => r.EndNodeId == SourceNode!.Id)
                    .ToArray();
            }
        }
    }
}
