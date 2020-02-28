// using System.Collections.Generic;
// using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
// using Neo4j.Driver;
//
// namespace DFC.ServiceTaxonomy.GraphLookup.Commands
// {
//     //IReadCommand with resultOperation?
//     public class LookupCommand : ICommand
//     {
//         public string? DisplayFieldSearchTerm { get; set; }
//
//         public string? NodeLabel { get; set; }
//         public string? DisplayFieldName { get; set; }
//         public string? ValueFieldName { get; set; }
//
//         private const string displayField = "d";
//         private const string valueField = "v";
//
//         public void CheckIsValid() => throw new System.NotImplementedException();
//
//         public Query Query
//         {
//             get
//             {
//                 return new Query($@"match (n:{NodeLabel})
// where toLower(n.{DisplayFieldName}) starts with toLower('{DisplayFieldSearchTerm}')
// return n.{DisplayFieldName} as {displayField}, n.{ValueFieldName} as {valueField}
// order by toLower({displayField})
// limit 50");
//             }
//         }
//
//         public void ValidateResults(List<IRecord> records, IResultSummary resultSummary) => throw new System.NotImplementedException();
//     }
// }
