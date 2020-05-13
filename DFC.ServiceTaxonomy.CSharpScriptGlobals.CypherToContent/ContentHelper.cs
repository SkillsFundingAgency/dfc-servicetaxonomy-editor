using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Exceptions;
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

//todo: if this still uses too much memory, we could probably remove the need for OrchardCore.ContentManagement, just include YesSql, and duplicate everything we need, which doesn't seem too much :)
//todo: implement MetadataReferenceResolver and don't load what we don't need
//todo: could have occupation label specific helper and load all into dictionary first, so don't have all the dependencies
//todo: look for specific pattern and don't invoke csharp compiler

// importing 1 occupation recipe (using broken out helper, CreateDelegate and batched GC) had max mem usage of 2.31gb, but heap generation gen 2 was steadily rising, ending up at 287mb
// if that keeps rising, may scupper a complete import
// using boken out helper, batched GC, and original CSharpScript.EvaluateAsync peaked at 3.3gb

namespace DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent
{
    public class ContentHelper : IContentHelper
    {
        private readonly ISession _session;

        public ContentHelper(ISession session)
        {
            _session = session;
        }

        public async Task<string> GetContentItemIdByDisplayText(string contentType, string displayText)
        {
            var query = _session.Query<ContentItem, ContentItemIndex>();

            query = query.With<ContentItemIndex>(x => x.DisplayText == displayText);

            query = query.With<ContentItemIndex>(x => x.Published);

            //check content type exists? or just let query fail?
            // var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.Options.SelectedContentType);
            // if (contentTypeDefinition == null)
            //     return NotFound();

            query = query.With<ContentItemIndex>(x => x.ContentType == contentType);

            ContentItem contentItem = await query.FirstOrDefaultAsync();

            if (contentItem == null)
            {
                throw new ContentHelperException($"Unable to get content item of type '{contentType}' with DisplayText '{displayText}'.");
            }

            return $"{contentItem.ContentItemId}";
        }
    }
}
