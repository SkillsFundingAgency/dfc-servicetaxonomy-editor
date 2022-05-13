using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Extensions;

namespace DFC.ServiceTaxonomy.GraphSync.Models
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
                return new Query(Command ?? string.Empty);
            }
        }

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            // empty
        }
    }
}

