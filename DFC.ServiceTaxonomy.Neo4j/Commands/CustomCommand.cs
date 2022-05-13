using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Commands.Implementation;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public class CustomCommand : ICustomCommand
    {
        public string? Command { get; set; }

        public List<string> ValidationErrors()
        {
            var errors = new List<string>();

            if (Command == null)
                errors.Add($"{nameof(Command)} is null.");

            return errors;
        }

        public Query Query
        {
            get
            {
                this.CheckIsValid();
                return new Query(Command);
            }
        }

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            // empty
        }
    }
}
