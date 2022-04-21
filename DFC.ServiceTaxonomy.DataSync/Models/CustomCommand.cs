using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.Extensions;
using DFC.ServiceTaxonomy.DataSync.Interfaces;

namespace DFC.ServiceTaxonomy.DataSync.Models
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

