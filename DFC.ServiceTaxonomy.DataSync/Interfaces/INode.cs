using System;
using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    public interface INode : IEntity, IEquatable<INode>
    {
        /// <summary>
        /// Gets the lables of the node.
        /// </summary>
        IReadOnlyList<string> Labels { get; }
    }
}
