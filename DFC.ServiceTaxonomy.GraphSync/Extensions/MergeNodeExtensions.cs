using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;


namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class MergeNodeExtensions
    {
        // where should these live? extension methods harder to test
        //todo: unit tests for these

        [return: MaybeNull]
        public static T AddProperty<T>(
            this IMergeNodeCommand mergeNodeCommand,
            string propertyName,
            JsonObject content)
        {
            return mergeNodeCommand.AddProperty<T>(propertyName, content, propertyName);
        }

        [return: MaybeNull]
        public static T AddProperty<T>(
            this IMergeNodeCommand mergeNodeCommand,
            string nodePropertyName,
            JsonObject content,
            string contentPropertyName)
        {
            T value;
            JsonValue? jvalue = (JsonValue?)content[contentPropertyName];

            if (jvalue == null)
                return default;

            switch (jvalue.GetValueKind())
            {
                case JsonValueKind.Null:
                    mergeNodeCommand.Properties.Add(nodePropertyName, null);
                    return default;
                case JsonValueKind.String:
                    if (DateTime.TryParseExact(jvalue.GetValue<string>(), "yyyy-MM-ddTHH:mm:ssK",
                            CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime utcTime))
                    {
                        value = (T)(object)utcTime;
                        mergeNodeCommand.Properties.Add(nodePropertyName, value);
                        break;
                    }
                    else
                    {
#pragma warning disable S907 // "goto" statement should not be used
                        goto default;
#pragma warning restore S907 // "goto" statement should not be used
                    }
                default:
                    if (typeof(T) == typeof(bool) && bool.TryParse(jvalue.ToString(), out bool boolValue))
                        value = (T) Convert.ChangeType(boolValue, typeof(T)) ;
                    else
                        value = jvalue.Value<T>()!;
                    if (value == null)
                        throw new InvalidCastException($"Could not convert content property {jvalue} to type {typeof(T)}");
                    mergeNodeCommand.Properties.Add(nodePropertyName, value);
                    break;
            }
                        
            return value;
        }

        public static List<T>? AddArrayProperty<T>(
            this IMergeNodeCommand mergeNodeCommand,
            string propertyName,
            JsonObject content)
        {
            return mergeNodeCommand.AddArrayProperty<T>(propertyName, content, propertyName);
        }

        public static List<T>? AddArrayProperty<T>(
            this IMergeNodeCommand mergeNodeCommand,
            string nodePropertyName,
            JsonObject content,
            string contentPropertyName)
        {
            List<T>? values;
            JsonArray? jarray = content[contentPropertyName]!.GetValue<JsonArray>();

            values = AddArrayProperty<T>(mergeNodeCommand, nodePropertyName, jarray);
            return values;

        }

        public static List<T>? AddArrayProperty<T>(
           this IMergeNodeCommand mergeNodeCommand,
           string nodePropertyName,
           JsonArray? jarray)
        {
            List<T>? values;
            if (jarray != null && jarray.GetValueKind() != JsonValueKind.Null)
            {
                values = jarray.ToObject<List<T>>();

                if (values == null)
                    throw new InvalidCastException($"Could not convert content property array {jarray} to type IEnumerable<{typeof(T)}>");

                mergeNodeCommand.Properties.Add(nodePropertyName, values);
            }
            else
            {
                values = default;
            }

            return values;
        }

        public static string[] AddArrayPropertyFromMultilineString(
            this IMergeNodeCommand mergeNodeCommand,
            string nodePropertyName,
            JsonObject content,
            string contentPropertyName)
        {
            string[]? valueStrings;
            var values = content[contentPropertyName];
            if (values != null && values.GetValueKind() != JsonValueKind.Null)
            {
                valueStrings = values.Value<string>()?.Split("\r\n") ?? new string[0];
            }
            else
            {
                valueStrings = new string[0];
            }

            mergeNodeCommand.Properties.Add(nodePropertyName, valueStrings);

            return valueStrings;
        }
    }
}
