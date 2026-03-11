using System.Collections.Generic;
using System.Text.Json.Nodes;
using DFC.ServiceTaxonomy.Taxonomies.Fields;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace DFC.ServiceTaxonomy.Taxonomies.GraphQL
{
    public class TaxonomyFieldQueryObjectType : ObjectGraphType<TaxonomyField>
    {
        public TaxonomyFieldQueryObjectType()
        {
            Name = nameof(TaxonomyField);

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>("termContentItemIds")
                .Description("term content item ids")
                .PagingArguments()
                .Resolve(x =>
                {
                    return x.Page(x.Source.TermContentItemIds);
                });

            Field<StringGraphType, string>("taxonomyContentItemId")
                .Description("taxonomy content item id")
                .Resolve(x =>
                {
                    return x.Source.TaxonomyContentItemId;
                });

            Field<ListGraphType<ContentItemInterface>, List<ContentItem>>("termContentItems")
                .Description("the term content items")
                .PagingArguments()
                .ResolveLockedAsync(async x =>
                {
                    var ids = x.Page(x.Source.TermContentItemIds);
                    var contentManager = x.RequestServices!.GetService<IContentManager>();

                    var taxonomy = await contentManager.GetAsync(x.Source.TaxonomyContentItemId);

                    if (taxonomy == null)
                    {
                        return null;
                    }

                    var terms = new List<ContentItem>();

                    foreach (var termContentItemId in ids)
                    {
                        var term = TaxonomyOrchardHelperExtensions.FindTerm(taxonomy.Content.TaxonomyPart.Terms as JsonArray, termContentItemId);
                        terms.Add(term);
                    }

                    return terms;
                });

             Field<ContentItemInterface, ContentItem>("taxonomyContentItem")
                .Description("the taxonomy content item")
                .ResolveLockedAsync(async x =>
                {
                    var contentManager = x.RequestServices!.GetService<IContentManager>();
                    return await contentManager.GetAsync(x.Source.TaxonomyContentItemId);
                });
        }
    }
}
