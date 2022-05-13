using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface IRecord
    {
        //
        // Summary:
        //     Gets the value at the given index.
        //
        // Parameters:
        //   index:
        //     The index
        //
        // Returns:
        //     The value specified with the given index.
        object this[int index] { get; }
        //
        // Summary:
        //     Gets the value specified by the given key.
        //
        // Parameters:
        //   key:
        //     The key
        //
        // Returns:
        //     the value specified with the given key.
        object this[string key] { get; }

        //
        // Summary:
        //     Gets the key and value pairs in a System.Collections.Generic.IReadOnlyDictionary`2.
        IReadOnlyDictionary<string, object> Values { get; }
        //
        // Summary:
        //     Gets the keys in a System.Collections.Generic.IReadOnlyList`1.
        IReadOnlyList<string> Keys { get; }
    }
}
