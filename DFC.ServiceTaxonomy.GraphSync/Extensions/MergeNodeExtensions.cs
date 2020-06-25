using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class MergeNodeExtensions
    {
        // either here, or graphsynchelper?
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
            if (jvalue != null && jvalue.Type != JTokenType.Null)
            {
                value = jvalue.Value<T>();

                if (value == null)
                    throw new InvalidCastException($"Could not convert content property {jvalue} to type {typeof(T)}");

                mergeNodeCommand.Properties.Add(nodePropertyName, value);
            }
            else
            {
                value = default;
            }

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
    }
}
