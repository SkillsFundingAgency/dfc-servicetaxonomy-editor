using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface ICommand
    {
        void CheckIsValid();

        Query Query { get; }

        void ValidateResults(List<IRecord> records, IResultSummary resultSummary);
    }
}
