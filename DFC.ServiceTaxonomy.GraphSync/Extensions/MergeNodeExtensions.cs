using System;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class MergeNodeExtensions
    {
        // either here, or graphsynchelper?
        //todo: unit tests for these

        public static string? AddProperty(
            this IMergeNodeCommand mergeNodeCommand,
            string propertyName,
            JObject content)
        {
            return mergeNodeCommand.AddProperty(propertyName, content, propertyName);
        }

        public static string? AddProperty(
            this IMergeNodeCommand mergeNodeCommand,
            string nodePropertyName,
            JObject content,
            string contentPropertyName)
        {
            return AddPropertyInternal<string>(mergeNodeCommand, nodePropertyName, content, contentPropertyName);
        }

        public static string? AddProperty<T>(
            this IMergeNodeCommand mergeNodeCommand,
            string nodePropertyName,
            JObject content,
            string contentPropertyName)
        {
            return AddPropertyInternal<T>(mergeNodeCommand, nodePropertyName, content, contentPropertyName);
        }

        private static string? AddPropertyInternal<T>(
            IMergeNodeCommand mergeNodeCommand,
            string nodePropertyName,
            JObject content,
            string contentPropertyName)
        {
            object? value;
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
                value = null;
            }

            return value?.ToString();
        }
    }
}
