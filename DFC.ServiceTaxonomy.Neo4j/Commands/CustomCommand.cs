using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands
{
    public class CustomCommand : ICustomCommand
    {
        public string? Command { get; set; }


        //todo: change to ValidationErrors, return errors & don't throw. throw in query if any validation errors returned
        public void CheckIsValid()
        {
            if (Command == null)
                throw new InvalidOperationException($"{nameof(Command)} is null.");
        }

        public Query Query
        {
            get
            {
                CheckIsValid();
                return new Query(Command);
            }
        }

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            // empty
        }
    }

}
