// using System.Collections.Generic;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
// using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
// using DFC.ServiceTaxonomy.Neo4j.Commands;
// using DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands.Interfaces;
// using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
// using Neo4j.Driver;
//
// namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands
// {
//     // inject IMergeContentItemNodeCommand into mergegraphsync
//     // or if published, activateserice replacement when pub
//     // need to set contentitemversion
//     // need to populate with result of querying draft relationships
//     public class MergeContentItemNodeCommand : MergeNodeCommand, IMergeContentItemNodeCommand
//     {
//         private readonly IReplaceRelationshipsCommand _replaceRelationshipsCommand;
//         public IContentItemVersion? ContentItemVersion { get; set; }
//
//         public MergeContentItemNodeCommand(IReplaceRelationshipsCommand replaceRelationshipsCommand)
//         {
//             _replaceRelationshipsCommand = replaceRelationshipsCommand;
//         }
//
//         public override List<string> ValidationErrors()
//         {
//             List<string> baseValidationErrors = base.ValidationErrors();
//
//             //todo: add our errors
//
//             return baseValidationErrors;
//         }
//
//         public override Query Query
//         {
//             get
//             {
//                 Query baseQuery = base.Query;
//
//                 if (ContentItemVersion!.GraphReplicaSetName == GraphReplicaSetNames.Preview)
//                     return baseQuery;
//
//                 return new Query(baseQuery.Text, baseQuery.Parameters);
//             }
//         }
//
//         public static implicit operator Query(MergeContentItemNodeCommand c) => c.Query;
//
//         public override void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
//         {
//
//         }
//     }
// }
