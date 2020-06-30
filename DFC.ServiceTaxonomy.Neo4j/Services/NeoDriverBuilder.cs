// using System.Collections.Generic;
// using System.Linq;
// using DFC.ServiceTaxonomy.Neo4j.Configuration;
// using DFC.ServiceTaxonomy.Neo4j.Models;
// using DFC.ServiceTaxonomy.Neo4j.Models.Interface;
// using Microsoft.Extensions.Options;
// using Neo4j.Driver;
//
// namespace DFC.ServiceTaxonomy.Neo4j.Services
// {
//     public class NeoDriverBuilder : INeoDriverBuilder
//     {
//         private readonly IOptionsMonitor<Neo4jConfiguration> _neo4JConfigurationOptions;
//         private readonly ILogger _logger;
//
//         public NeoDriverBuilder(IOptionsMonitor<Neo4jConfiguration> neo4jConfigurationOptions, ILogger logger)
//         {
//             _neo4JConfigurationOptions = neo4jConfigurationOptions;
//             _logger = logger;
//         }
//         public IEnumerable<INeoDriver> Build()
//         {
//             //TODO: GroupBy clause shouldn't be needed, but configuration provides duplicates for some reason
//             //Wrap the driver creation process to enable mocking
//             //todo: enabled?
//             //todo: allow config changes on the fly?
//             //todo: throw helpful exception if config bad, + log non-secret config
//             return _neo4JConfigurationOptions.CurrentValue.Endpoints
//                 //.GroupBy(y => y.Uri)
//                 .Select(e => new NeoDriver(GraphDatabase.Driver(
//                     e.Uri, AuthTokens.Basic(e.Username, e.Password),
//                     o => o.WithLogger(_logger)), z.FirstOrDefault().Uri));
//
//             //todo: neo4 doesn't encrypt by default (3 did), see https://neo4j.com/docs/driver-manual/current/client-applications/
//             // TrustStrategy
//             //o=>o.WithEncryptionLevel(EncryptionLevel.None));
//
//             //todo: create distinct gendpoints for all graphs dictionary
//
//             // return _neo4JConfigurationOptions.CurrentValue.Graphs.Select(
//             //     g )
//         }
//     }
// }
