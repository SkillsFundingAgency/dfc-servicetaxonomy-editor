using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    /*
{
  "taxtest": {
    "sitesection": {
      "TagNames": [
        "Help"
      ],
      "TaxonomyContentItemId": "422h7x37sb3y96ymarrpf13ew6",    <- don't think we need anything from here
      "TermContentItemIds": [
        "420kb1j83dxqr3b36wgbncmzxs"    <- these should sync separatly (as long as they have a graph sync part), then we can create a relationship to it. can contain other fields than title, e.g. could have bool 'Create Contents Page)
        // share code with contentpickerfield
        // just go with default relationship name ie has<Taxonomygieldname> eg hassitesection
        // probably no need to save tagnames? or just add them as properties anyway? easier for clients, but might get out of sync
      ]
    }
  },
  "GraphSyncPart": {
    "Text": "5a034199-8ae3-47d9-a91a-75794e247614"
  }
}
     */
    public class TaxonomyFieldGraphSyncer : IContentFieldGraphSyncer
    {
        public string FieldTypeName => "TaxonomyField";

        private const string TermContentItemIds = "TermContentItemIds";

        public Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            return Task.CompletedTask;
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField,
            IContentPartFieldDefinition contentPartFieldDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts)
        {
            return Task.FromResult((true, ""));
        }
    }
}
