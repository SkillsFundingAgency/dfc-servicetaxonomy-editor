using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Taxonomy
{
    //also need termpart sync (only if we want a 2 way relationship?)
    /*
"TaxonomyPart": {
    "Terms": [
      {
        "ContentItemId": "420kb1j83dxqr3b36wgbncmzxs",
        "ContentItemVersionId": null,
        "ContentType": "SiteSection",
        "DisplayText": "Help",
        "Latest": false,
        "Published": false,
        "ModifiedUtc": "2020-06-16T10:19:36.1223459Z",
        "PublishedUtc": null,
        "CreatedUtc": null,
        "Owner": null,
        "Author": "admin",
        "SiteSection": {
          "CreateContentsPage": {
            "Value": true
          }
        },
        "GraphSyncPart": {
          "Text": "c8875c91-8269-4eab-9131-a534bb21aae4"
        },
        "TitlePart": {
          "Title": "Help"
        },
        "TermPart": {
          "TaxonomyContentItemId": "422h7x37sb3y96ymarrpf13ew6"
        },
        "@WeldedPartSettings": {
          "TermPart": {}
        }
      },
      {
        "ContentItemId": "4d11hr2pxn2te2c7pw2n06hf5m",
        "ContentItemVersionId": null,
        "ContentType": "SiteSection",
        "DisplayText": "Contact Us",
        "Latest": false,
        "Published": false,
        "ModifiedUtc": "2020-06-16T10:19:46.9984754Z",
        "PublishedUtc": null,
        "CreatedUtc": null,
        "Owner": null,
        "Author": "admin",
        "SiteSection": {},
        "GraphSyncPart": {
          "Text": "490724ee-eb7e-415f-b4d3-5ce4262ef1a7"
        },
        "TitlePart": {
          "Title": "Contact Us"
        },
        "TermPart": {
          "TaxonomyContentItemId": "422h7x37sb3y96ymarrpf13ew6"
        },
        "@WeldedPartSettings": {
          "TermPart": {}
        }
      },
      {
        "ContentItemId": "4h58ty0hvxnc05ffsa1vkj0bdc",
        "ContentItemVersionId": null,
        "ContentType": "SiteSection",
        "DisplayText": "new through tax ui",
        "Latest": false,
        "Published": false,
        "ModifiedUtc": "2020-06-16T11:47:54.45765Z",
        "PublishedUtc": null,
        "CreatedUtc": null,
        "Owner": null,
        "Author": "admin",
        "SiteSection": {},
        "GraphSyncPart": {
          "Text": null
        },
        "TitlePart": {},
        "TermPart": {
          "TaxonomyContentItemId": "422h7x37sb3y96ymarrpf13ew6"
        },
        "@WeldedPartSettings": {
          "TermPart": {}
        }
      },
      {
        "ContentItemId": "4kpwv1r28yc8py4sdfegcatbc0",
        "ContentItemVersionId": null,
        "ContentType": "SiteSection",
        "DisplayText": "new1",
        "Latest": false,
        "Published": false,
        "ModifiedUtc": "2020-06-16T11:50:03.8527353Z",
        "PublishedUtc": null,
        "CreatedUtc": null,
        "Owner": null,
        "Author": "admin",
        "SiteSection": {},
        "GraphSyncPart": {
          "Text": null
        },
        "TitlePart": {},
        "TermPart": {
          "TaxonomyContentItemId": "422h7x37sb3y96ymarrpf13ew6"
        },
        "@WeldedPartSettings": {
          "TermPart": {}
        }
      }
    ],
    "TermContentType": "SiteSection"
  },

     */
    public class TaxonomyPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly ITaxonomyPartEmbeddedContentItemsGraphSyncer _taxonomyPartEmbeddedContentItemsGraphSyncer;
        public string PartName => nameof(TaxonomyPart);

        private const string ContainerName = "Terms";
        private const string TermContentTypePropertyName = "TermContentType";

        public TaxonomyPartGraphSyncer(
            ITaxonomyPartEmbeddedContentItemsGraphSyncer taxonomyPartEmbeddedContentItemsGraphSyncer)
        {
            _taxonomyPartEmbeddedContentItemsGraphSyncer = taxonomyPartEmbeddedContentItemsGraphSyncer;
        }

        public async Task AddSyncComponents(JObject content,
            ContentItem contentItem,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            await _taxonomyPartEmbeddedContentItemsGraphSyncer.AddSyncComponents(
                (JArray?)content[ContainerName],
                replaceRelationshipsCommand,
                graphSyncHelper);

            // useful if there are no terms yet?
            mergeNodeCommand.AddProperty(TermContentTypePropertyName, content);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint)
        {
            (bool validated, string failureReason) =
                await _taxonomyPartEmbeddedContentItemsGraphSyncer.ValidateSyncComponent(
                    (JArray?)content[ContainerName],
                    nodeWithOutgoingRelationships,
                    graphValidationHelper,
                    expectedRelationshipCounts,
                    endpoint);

            if (!validated)
                return (validated, failureReason);

            return graphValidationHelper.StringContentPropertyMatchesNodeProperty(
                TermContentTypePropertyName,
                content,
                TermContentTypePropertyName,
                nodeWithOutgoingRelationships.SourceNode);
        }
    }
}
