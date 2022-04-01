using System;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class ValueExtensions
    {
        //
        // Summary:
        //     A helper method to explicitly cast the value streamed back via Bolt to a local
        //     type, with default fallback value.
        //
        // Parameters:
        //   value:
        //     The value that streamed back via Bolt protocol, e.g.Neo4j.Driver.IEntity.Properties.
        //
        //   defaultValue:
        //     Returns this value if the the value is null
        //
        // Type parameters:
        //   T:
        //     Supports for the following types (or nullable version of the following types
        //     if applies): System.Int16, System.Int32, System.Int64, System.Single, System.Double,
        //     System.SByte, System.UInt16, System.UInt32, System.UInt64, System.Byte, System.Char,
        //     System.Boolean, System.String, System.Collections.Generic.List`1, Neo4j.Driver.INode,
        //     Neo4j.Driver.IRelationship, Neo4j.Driver.IPath. Undefined support for other types
        //     that are not listed above. No support for user-defined types, e.g. Person, Movie.
        //
        // Returns:
        //     The value of specified return type.
        //
        // Remarks:
        //     Throws System.InvalidCastException if the specified cast is not possible.
        public static T As<T>(this object value, T defaultValue)
        {
            return defaultValue;
        }
        //
        // Summary:
        //     A helper method to explicitly cast the value streamed back via Bolt to a local
        //     type.
        //
        // Parameters:
        //   value:
        //     The value that streamed back via Bolt protocol, e.g.Neo4j.Driver.IEntity.Properties.
        //
        // Type parameters:
        //   T:
        //     Supports for the following types (or nullable version of the following types
        //     if applies): System.Int16, System.Int32, System.Int64, System.Single, System.Double,
        //     System.SByte, System.UInt16, System.UInt32, System.UInt64, System.Byte, System.Char,
        //     System.Boolean, System.String, System.Collections.Generic.List`1, Neo4j.Driver.INode,
        //     Neo4j.Driver.IRelationship, Neo4j.Driver.IPath. Undefined support for other types
        //     that are not listed above. No support for user-defined types, e.g. Person, Movie.
        //
        // Returns:
        //     The value of specified return type.
        //
        // Remarks:
        //     Throws System.InvalidCastException if the specified cast is not possible.
        public static T As<T>(this object value)
        #pragma warning disable CS8603 // Possible null reference return.
        {
            if (value is JValue jValue)
            {
                return jValue.ToObject<T>();
            }

            var toType = typeof(T);

            if ((value is int || value is bool || value is double || value is decimal || value is float) && toType == typeof(string))
            {
                return (T)(object)value.ToString()!;
            }

            if (value is string)
            {
                return (T)value;
            }

            return (T)Activator.CreateInstance(toType, new object[] { 1 })!;
        }
        #pragma warning restore CS8603 // Possible null reference return.
    }
}
