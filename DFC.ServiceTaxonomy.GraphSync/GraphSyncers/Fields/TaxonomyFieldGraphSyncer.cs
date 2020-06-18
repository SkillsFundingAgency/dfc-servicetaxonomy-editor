using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
//using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    //todo: do we need @WeldedPartSettings?
    //todo: graphsyncsettings for term for uri?
    // might have to allow override somehow.

    /*
     the content type created for the terms doesn't behave like other content types
     content items created when terms are added through the taxonomy ui aren't listed in the contents list (even though the type is listable)
      content items created when terms are added through the taxonomy ui don't trigger publication, so they don't get synced
      content items of the term type can be created as usual, and are synced when published, but aren't available as terms

      what we'll probably have to do is sync the term content type when we sync the taxonomy field
        update creating new term though ui creates new taxonomy item with terms embedded like bag part, which explains the above
        can we edit taxonomy to add a graphsyncpart though?

        we can add a graph sync part to the taxonomy type, but..
        can we set that up through a recipe?
        the graphsyncpart content comes through empty after adding to the part and adding a new term though the taxonomy ui
        but graphsyncpart text comes through ok after publishing
        need to see what happens when taxonomy is created/updated through recipe (if that's possible)

        sitesection (term content type) in taxonomy terms back has the display text, but a blank title part when the term is added when editing the page item, but title is there when created from the taxonomy item editor!!
        could hack in the title when syncing (but saving oc content item may be propblematic), or else change title part to ignore missing title and set from display text after recursed terms??


(Term)<-[hasSiteSection]-(Page)-[hasTaxonomy]->(Taxonomy)-\
    ^----------[hasTerm/SiteSection]----------------------/

why like this?
mirrors content structure in oc
get terms synced from taxonomy (fits in better with current sync scheme)
can see all possible terms for page, even if current page doesn't have all instances
can also have tagnames as properties of page for ease of retrieval (and matches content, so less hackery)

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


{
  "TitlePart": {
    "Title": "Site Section Taxonomy"
  },
  "AliasPart": {
    "Alias": "site-section-taxonomy"
  },
  "AutoroutePart": {
    "Path": "site-section-taxonomy",
    "SetHomepage": false,
    "Disabled": false,
    "RouteContainedItems": false,
    "Absolute": false
  },
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
  "GraphSyncPart": {}
}


{
  "TagNames": [
    "Help"
  ],
  "TaxonomyContentItemId": "4eembshqzx66drajtdten34tc8",
  "TermContentItemIds": [
    "4vx6b17n8c1j3tgjherk977ed2"
  ]
}
     */
    public class TaxonomyFieldGraphSyncer : IContentFieldGraphSyncer
    {
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        public string FieldTypeName => "TaxonomyField";

        private const string TagNames = "TagNames";
        private const string TaxonomyContentItemId = "TaxonomyContentItemId";
        private const string TermContentItemIds = "TermContentItemIds";

        public TaxonomyFieldGraphSyncer(
            IContentManager contentManager,
            IServiceProvider serviceProvider)
        {
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
        }

        public async Task AddSyncComponents(
            JObject contentItemField,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentPartFieldDefinition contentPartFieldDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            //todo: share code with contentpickerfield

            string? taxonomyContentItemId = contentItemField[TaxonomyContentItemId]?.Value<string>();
            //todo: null?

            ContentItem taxonomyContentItem = await _contentManager.GetAsync(taxonomyContentItemId!, VersionOptions.Published);
            var taxonomyPartContent = taxonomyContentItem.Content[nameof(TaxonomyPart)];
            string termContentType = taxonomyPartContent["TermContentType"];

            string relationshipType = $"has{termContentType}";

            //todo requires 'picked' part has a graph sync part
            // add to docs & handle picked part not having graph sync part or throw exception

            JArray? contentItemIdsJArray = (JArray?)contentItemField[TermContentItemIds];
            if (contentItemIdsJArray == null || !contentItemIdsJArray.HasValues)
                return; //todo:

            IEnumerable<string> contentItemIds = contentItemIdsJArray.Select(jtoken => jtoken.Value<string>());

            //todo: term content items are embedded in the taxonomy (and can't be GetAsync'ed)

            // GetAsync should be returning ContentItem? as it can be null
            // IEnumerable<Task<ContentItem>> destinationContentItemsTasks =
            //     contentItemIds.Select(async contentItemId =>
            //         await _contentManager.GetAsync(contentItemId, VersionOptions.Published));
            //
            // ContentItem?[] destinationContentItems = await Task.WhenAll(destinationContentItemsTasks);
            //
            // IEnumerable<ContentItem?> foundDestinationContentItems =
            //     destinationContentItems.Where(ci => ci != null);
            //
            // if (foundDestinationContentItems.Count() != contentItemIds.Count())
            //     throw new GraphSyncException($"Missing picked content items. Looked for {string.Join(",", contentItemIds)}. Found {string.Join(",", foundDestinationContentItems)}. Current merge node command: {mergeNodeCommand}.");

            // warning: we should logically be passing an IGraphSyncHelper with its ContentType set to pickedContentType
            // however, GetIdPropertyValue() doesn't use the set ContentType, so this works
            // IEnumerable<object> foundDestinationNodeIds =
            //     foundDestinationContentItems.Select(ci => GetNodeId(ci!, graphSyncHelper));

            IGraphSyncHelper relatedGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();
            relatedGraphSyncHelper.ContentType = termContentType;

            //todo: handle missing graphsynchelper. extract into GetNodeId method
            JArray taxonomyTermsContent = (JArray)taxonomyPartContent["Terms"];
            IEnumerable<object> foundDestinationNodeIds = contentItemIds.Select(tid =>
                GetNodeId(tid, taxonomyTermsContent, relatedGraphSyncHelper));

            IEnumerable<string> destNodeLabels = await relatedGraphSyncHelper.NodeLabels();

            replaceRelationshipsCommand.AddRelationshipsTo(
                relationshipType,
                null,
                destNodeLabels,
                relatedGraphSyncHelper!.IdPropertyName(),
                foundDestinationNodeIds.ToArray());

            // add relationship to taxonomy
            //string taxonomyAlias = taxonomyContentItem.Content[nameof(AliasPart)]["Alias"];
            string taxonomyName = taxonomyContentItem.DisplayText.Replace(" ", "");
            string taxonomyRelationshipType = $"has{taxonomyName}Taxonomy";

            relatedGraphSyncHelper.ContentType = taxonomyContentItem.ContentType;
            destNodeLabels = await relatedGraphSyncHelper.NodeLabels();
            object taxonomyIdValue = relatedGraphSyncHelper.GetIdPropertyValue(taxonomyContentItem.Content[nameof(GraphSyncPart)]);

            replaceRelationshipsCommand.AddRelationshipsTo(
                taxonomyRelationshipType,
                null,
                destNodeLabels,
                relatedGraphSyncHelper!.IdPropertyName(),
                taxonomyIdValue);
        }

        private object GetNodeId(string termContentItemId, JArray taxonomyTermsContent, IGraphSyncHelper termGraphSyncHelper)
        {
            JObject termContentItem = (JObject)taxonomyTermsContent.First(token => token["ContentItemId"]?.Value<string>() == termContentItemId);
            return termGraphSyncHelper.GetIdPropertyValue(termContentItem[nameof(GraphSyncPart)]!);
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
