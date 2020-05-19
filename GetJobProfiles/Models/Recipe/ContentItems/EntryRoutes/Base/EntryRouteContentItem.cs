// using System.Collections.Generic;
// using GetJobProfiles.Models.Recipe.ContentItems.Base;
// using GetJobProfiles.Models.Recipe.Fields;
// using GetJobProfiles.Models.Recipe.Parts;
//
// namespace GetJobProfiles.Models.Recipe.ContentItems.EntryRoutes.Base
// {
//     public class EntryRouteContentItem : ContentItem
//     {
//         public EntryRouteContentItem(string contentType, string timestamp, IEnumerable<string> description,
//             string contentItemId = null)
//             : base(contentType, null, timestamp, contentItemId)
//         {
//             GraphSyncPart = new GraphSyncPart(contentType);
//             EponymousPart = new EntryRoutePart
//             {
//                 Description = new HtmlField(description)
//             };
//         }
//
//         public virtual EntryRoutePart EponymousPart { get; set; }
//         public GraphSyncPart GraphSyncPart { get; set; }
//     }
//
//     public class EntryRoutePart
//     {
//         public HtmlField Description { get; set; }
//     }
// }
