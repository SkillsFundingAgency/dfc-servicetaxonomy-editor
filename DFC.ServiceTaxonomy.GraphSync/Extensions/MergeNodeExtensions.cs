using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            JObject content)
        {
            return mergeNodeCommand.AddProperty<T>(propertyName, content, propertyName);
        }

        [return: MaybeNull]
        public static T AddProperty<T>(
            this IMergeNodeCommand mergeNodeCommand,
            string nodePropertyName,
            JObject content,
            string contentPropertyName)
        {
            T value;
            JValue? jvalue = (JValue?)content[contentPropertyName];

            if (jvalue == null)
                return default;

            switch (jvalue.Type)
            {
                case JTokenType.Null:
                    return default;
                case JTokenType.Date:
                    string utcdate = jvalue.ToString(Formatting.None).Trim('"');
                    DateTime utcTime = DateTime.ParseExact(utcdate, "yyyy-MM-ddTHH:mm:ssK",
                        CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

                    value = (T)(object)utcTime;
                    break;
                default:
                    value = jvalue.Value<T>();
                    if (value == null)
                        throw new InvalidCastException($"Could not convert content property {jvalue} to type {typeof(T)}");
                    break;
            }
            mergeNodeCommand.Properties.Add(nodePropertyName, value);
            return value;
        }

        public static List<T>? AddArrayProperty<T>(
            this IMergeNodeCommand mergeNodeCommand,
            string propertyName,
            JObject content)
        {
            return mergeNodeCommand.AddArrayProperty<T>(propertyName, content, propertyName);
        }

        public static List<T>? AddArrayProperty<T>(
            this IMergeNodeCommand mergeNodeCommand,
            string nodePropertyName,
            JObject content,
            string contentPropertyName)
        {
            List<T>? values;
            JArray? jarray = (JArray?)content[contentPropertyName];

            values = AddArrayProperty<T>(mergeNodeCommand, nodePropertyName, jarray);
            return values;

        }

        public static List<T>? AddArrayProperty<T>(
           this IMergeNodeCommand mergeNodeCommand,
           string nodePropertyName,
           JArray? content)
        {
            List<T>? values;
            JArray? jarray = content;
            if (jarray != null && jarray.Type != JTokenType.Null)
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
            JObject content,
            string contentPropertyName)
        {
            string[]? valueStrings;
            JToken? values = content[contentPropertyName];
            if (values != null && values.Type != JTokenType.Null)
            {
                valueStrings = values.Value<string>().Split("\r\n");
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
