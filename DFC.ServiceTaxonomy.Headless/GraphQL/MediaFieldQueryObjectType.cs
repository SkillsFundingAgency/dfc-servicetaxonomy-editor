using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Media.Fields;
using OrchardCore.Media;


//Custom Media Field object type overriding existing graphql object type to expose the media field alt text field
namespace DFC.ServiceTaxonomy.Headless.GraphQL
{
    public class MediaFieldQueryObjectType : ObjectGraphType<MediaField>
    {
        public MediaFieldQueryObjectType()
        {
            Name = "NCSMediaField";

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>()
                .Name("paths")
                .Description("the media paths")
                .PagingArguments()
                .Resolve(x =>
                {
                    if (x.Source?.Paths is null)
                    {
                        return Array.Empty<string>();
                    }
                    return x.Page(x.Source.Paths);
                });

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>()
                .Name("urls")
                .Description("the absolute urls of the media items")
                .PagingArguments()
                .Resolve(x =>
                {
                    if (x.Source?.Paths is null)
                    {
                        return Array.Empty<string>();
                    }
                    var paths = x.Page(x.Source.Paths);
                    var mediaFileStore = x.RequestServices.GetService<IMediaFileStore>();
                    return paths.Select(p => mediaFileStore.MapPathToPublicUrl(p));
                });

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>()
                .Name("mediaText")
                .Description("the media text")
                .PagingArguments()
                .Resolve(x =>
                {
                    if (x.Source?.MediaTexts is null)
                    {
                        return Array.Empty<string>();
                    }
                    return x.Page(x.Source.MediaTexts);
                });
        }
    }
}
