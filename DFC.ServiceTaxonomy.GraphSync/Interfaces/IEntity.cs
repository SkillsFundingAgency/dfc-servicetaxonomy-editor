using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface IEntity
    {
        //
        // Summary:
        //     Gets the value that has the specified key in Properties.
        //
        // Parameters:
        //   key:
        //     The key.
        //
        // Returns:
        //     The value specified by the given key in Properties.
        object this[string key] { get; }

        //
        // Summary:
        //     Gets the properties of the entity.
        IReadOnlyDictionary<string, object> Properties { get; }

        //
        // Summary:
        //     Get the identity as a System.Int64 number.
        long Id { get; }
    }
}
