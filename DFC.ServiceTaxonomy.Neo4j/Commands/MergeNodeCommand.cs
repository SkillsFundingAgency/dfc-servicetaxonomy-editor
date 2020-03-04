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
            if (NodeLabels == null)
                throw new InvalidOperationException($"{nameof(NodeLabels)} is null");

            if (!NodeLabels.Any())
                throw new InvalidOperationException($"No labels");

            if (IdPropertyName == null)
                throw new InvalidOperationException($"{nameof(IdPropertyName)} is null");
        }

        private Query CreateQuery()
        {
            CheckIsValid();

            return new Query(
                $"MERGE (n:{string.Join(':',NodeLabels)} {{ {IdPropertyName}:'{Properties[IdPropertyName!]}' }}) SET n=$properties RETURN ID(n)",
                new Dictionary<string,object> {{"properties", Properties}});
        }

        public Query Query => CreateQuery();

        public static implicit operator Query(MergeNodeCommand c) => c.Query;

        //todo: throw or combine all validation errors and throw aggregateexception, return errors?
        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            // nothing yet
            // validation can check return from query and/or counters are in range in result summary and/or notifications

            //todo: update query to return better validation failure indicator

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
