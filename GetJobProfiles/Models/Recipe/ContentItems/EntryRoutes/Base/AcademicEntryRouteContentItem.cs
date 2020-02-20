using GetJobProfiles.Models.API;
using GetJobProfiles.Models.Recipe.ContentItems.Base;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;

namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base
{
    public class AcademicEntryRouteContentItem : ContentItem
    {
        public AcademicEntryRouteContentItem(string contentType, AcademicEntryRoute entryRoute, string timestamp, string contentItemId = null)
            : base(contentType, null, timestamp, contentItemId)
        {
            EponymousPart = new AcademicEntryRoutePart
            {
                RelevantSubjects = new HtmlField(entryRoute.RelevantSubjects),
                FurtherInfo = new HtmlField(entryRoute.FurtherInformation)
            };
            GraphSyncPart = new GraphSyncPart(contentType);
        }

        //todo: might need to have an EponymousBagPart to pick up the correct content type name (or somesuch)
        //todo: prob gonna have to add json polymorphic converter too
        public virtual AcademicEntryRoutePart EponymousPart { get; set; }
        public GraphSyncPart GraphSyncPart { get; set; }
    }

    public class AcademicEntryRoutePart
    {
        public HtmlField RelevantSubjects { get; set; }
        public HtmlField FurtherInfo { get; set; }
        public ContentPicker RequirementsPrefix { get; set; }    //todo: just string?
        public ContentPicker Requirements { get; set; }
        public ContentPicker Links { get; set; }
    }
}

/*
 * {
  "ContentItems": [
    {
      "ContentItemId": "42f54e2p517vx483shgjbreyw3",
      "ContentItemVersionId": null,
      "ContentType": "UniversityRoute",
      "DisplayText": "University Route",
      "Latest": false,
      "Published": false,
      "ModifiedUtc": "2020-02-14T11:36:57.3030372Z",
      "PublishedUtc": null,
      "CreatedUtc": null,
      "Owner": null,
      "Author": "admin",
      "TitlePart": {
        "Title": "University Route"
      },
      "UniversityRoute": {
        "RelevantSubjects": {
          "Html": "<p>You could study for a degree in a subject like:</p><ul><li>zoology</li><li>animal ecology</li><li>animal behaviour</li><li>conservation</li></ul>"
        },
        "FurtherInfo": {
          "Html": "<p>You'll need a relevant postgraduate qualification like a master's degree or PhD for some jobs, particularly in research.<br></p>"
        },
        "RequirementsPrefix": {
          "ContentItemIds": [
            "46r3q8j08m47ytd4zpd459bqnp"
          ]
        },
        "Requirements": {
          "ContentItemIds": [
            "44psrmmfam5x5x96b71akmqk5w",
            "4gykd3k0zk9ac27aqdmm24vxck"
          ]
        },
        "Links": {
          "ContentItemIds": [
            "41rdx8tthvtsbxma8tm3a6zzm4",
            "4gw16rcda3qs74p725c3t9s05t",
            "4sn31w5f1dnmcvwg0zhw6949d3"
          ]
        }
      },
      "GraphSyncPart": {
        "Text": "http://nationalcareers.service.gov.uk/universityroute/f380d966-1ba4-4f49-befe-73f903e57a6d"
      }
    },
    {
      "ContentItemId": "4zsppznfgwzf8533ptb2qcs8mk",
      "ContentItemVersionId": null,
      "ContentType": "WorkRoute",
      "DisplayText": "Work Route",
      "Latest": false,
      "Published": false,
      "ModifiedUtc": "2020-02-14T11:36:57.3400392Z",
      "PublishedUtc": null,
      "CreatedUtc": null,
      "Owner": null,
      "Author": "admin",
      "TitlePart": {
        "Title": "Work Route"
      },
      "WorkRoute": {
        "Description": {
          "Html": "<p>work route info</p>"
        }
      },
      "GraphSyncPart": {
        "Text": "http://nationalcareers.service.gov.uk/workroute/bd080dc3-4aab-4ca0-be24-80acc6d8713c"
      }
    }
  ]
}
*/
