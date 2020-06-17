using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.OrchardCore.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields
{
    //todo: do we need @WeldedPartSettings?
    //todo: graphsyncsettings for term for uri?

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
        public string FieldTypeName => "TaxonomyField";

        private const string TagNames = "TagNames";
        private const string TaxonomyContentItemIds = "TaxonomyContentItemId";
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
