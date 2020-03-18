using System;
using System.IO;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.CSharpScripting
{
    public class ContentHelper : IContentHelper
    {
        private readonly ISession _session;

        public ContentHelper(ISession session)
        {
            _session = session;
        }

        public async Task<string> GetContentItemIdByDisplayText(string contentType, string displayText) //string lookupField, string lookupValue)
        {
            var query = _session.Query<ContentItem, ContentItemIndex>();

            query = query.With<ContentItemIndex>(x => x.DisplayText.Contains(displayText));

            // ???
            query = query.With<ContentItemIndex>(x => x.Published);

            //check content type exists? or just let query fail?
            // var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.Options.SelectedContentType);
            // if (contentTypeDefinition == null)
            //     return NotFound();

            query = query.With<ContentItemIndex>(x => x.ContentType == contentType);

            ContentItem contentItem = await query.FirstOrDefaultAsync();

            if (contentItem == null) //todo: what exception?
                throw new InvalidDataException($"Unable to get content item of type '{contentType}' with DisplayText '{displayText}'.");

//            return $"\"{contentItem.ContentItemId}\"";
            //where are the other quotes coming from?
            return $"{contentItem.ContentItemId}";
        }
    }
}
