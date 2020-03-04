using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    //todo: inject database and add execute?
    public class MergeNodeCommand : IMergeNodeCommand
    {
        public HashSet<string> NodeLabels { get; set; } = new HashSet<string>();
        public string? IdPropertyName { get; set; }
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public void CheckIsValid()
        {
            List<string> validationErrors = new List<string>();

            if (!NodeLabels.Any())
                validationErrors.Add($"Missing {nameof(NodeLabels)}.");

            if (IdPropertyName == null)
                validationErrors.Add($"{nameof(IdPropertyName)} is null.");

            if (validationErrors.Any())
                throw new InvalidOperationException(@$"{nameof(MergeNodeCommand)} not valid:
{string.Join(Environment.NewLine, validationErrors)}");
        }

        private Query CreateQuery()
        {
            CheckIsValid();

            // Query gracefully handles case when Properties == null
            return new Query(
                $"MERGE (n:{string.Join(':',NodeLabels)} {{ {IdPropertyName}:'{Properties[IdPropertyName!]}' }}) SET n=$properties RETURN ID(n)",
                new Dictionary<string,object> {{"properties", Properties}});
        }

        public Query Query => CreateQuery();

        public static implicit operator Query(MergeNodeCommand c) => c.Query;

        //todo: throw or combine all validation errors and throw aggregateexception, return errors?
        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            int expectedPropertyCount = Properties.Count, expectedLabelsAdded;
            switch (resultSummary.Counters.NodesCreated)
            {
                case 0:
                    expectedLabelsAdded = 0;
                    break;
                case 1:
                    expectedLabelsAdded = NodeLabels.Count;
                    // if we created the node, then the id property automatically gets created
                    ++expectedPropertyCount;
                    break;
                default:
                    throw new CommandValidationException($"Expecting no more than 1 node to be created.");
            }

            if (resultSummary.Counters.PropertiesSet != expectedPropertyCount)
                throw new CommandValidationException($"Expecting {expectedPropertyCount} properties to have been set, but {resultSummary.Counters.PropertiesSet} were actually set.");

            if (resultSummary.Counters.LabelsAdded != expectedLabelsAdded)
                throw new CommandValidationException($"Expected {expectedLabelsAdded} to be added, but {resultSummary.Counters.LabelsAdded} were actually added.");

            long? nodeId = records?.FirstOrDefault()?.Values.Values.FirstOrDefault()?.As<long?>();
            if (nodeId == null)
                throw new CommandValidationException($"Id of created node not returned");

            //todo: log id?
        }
    }
}
