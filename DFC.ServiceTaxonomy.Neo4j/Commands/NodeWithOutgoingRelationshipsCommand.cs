﻿using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    //todo: move into subfolder?
    public abstract class NodeWithOutgoingRelationshipsCommand : INodeWithOutgoingRelationshipsCommand
    {
        public HashSet<string> SourceNodeLabels { get; set; } = new HashSet<string>();
        public string? SourceIdPropertyName { get; set; }
        public object? SourceIdPropertyValue { get; set; }

        public IEnumerable<CommandRelationship> Relationships
        {
            get { return RelationshipsList; }
        }

        protected List<CommandRelationship> RelationshipsList { get; set; } = new List<CommandRelationship>();

        public void AddRelationshipsTo(
            string relationshipType,
            IReadOnlyDictionary<string, object>? properties,
            IEnumerable<string> destNodeLabels,
            string destIdPropertyName,
            params object[] destIdPropertyValues)
        {
            RelationshipsList.Add(new CommandRelationship(relationshipType, properties, destNodeLabels, destIdPropertyName,
                destIdPropertyValues));
        }

        public void AddRelationshipsTo(IEnumerable<CommandRelationship> commandRelationship)
        {
            RelationshipsList.AddRange(commandRelationship);
        }

        public virtual List<string> ValidationErrors()
        {
            List<string> validationErrors = new List<string>();

            if (!SourceNodeLabels.Any())
                validationErrors.Add($"Missing {nameof(SourceNodeLabels)}.");

            if (SourceIdPropertyName == null)
                validationErrors.Add($"{nameof(SourceIdPropertyName)} is null.");

            if (SourceIdPropertyValue == null)
                validationErrors.Add($"{nameof(SourceIdPropertyValue)} is null.");

            foreach (var relationship in RelationshipsList)
            {
                var relationshipValidationErrors = relationship.ValidationErrors;
                if (relationshipValidationErrors.Any())
                {
                    validationErrors.Add($"{relationship.RelationshipType??"<Null Type>"} relationship invalid ({string.Join(",", relationshipValidationErrors)})");
                }
            }

            return validationErrors;
        }

        public static implicit operator Query(NodeWithOutgoingRelationshipsCommand c) => c.Query;

        protected static string AllVariablesString(string variableBase, int ordinal) =>
            string.Join(',', Enumerable.Range(1, ordinal).Select(o => $"{variableBase}{o}"));

        public override string ToString()
        {
            return $@"(:{string.Join(':', SourceNodeLabels)} {{{SourceIdPropertyName}: '{SourceIdPropertyValue}'}})
Relationships:
{string.Join(Environment.NewLine, RelationshipsList)}";
        }

        public abstract Query Query { get; }
        public abstract void ValidateResults(List<IRecord> records, IResultSummary resultSummary);
    }
}
