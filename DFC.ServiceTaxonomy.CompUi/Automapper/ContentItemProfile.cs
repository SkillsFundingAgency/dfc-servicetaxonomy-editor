using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using DFC.ServiceTaxonomy.CompUi.Models;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.CompUi.Automapper
{
    [ExcludeFromCodeCoverage]
    public class ContentItemProfile : Profile
    {
        public ContentItemProfile()
        {
            CreateMap<ContentContextBase, Processing>()
                .ForMember(d => d.Author, x => x.MapFrom(s => s.ContentItem.Author))
                .ForMember(d => d.ContentType, x => x.MapFrom(s => s.ContentItem.ContentType))
                .ForMember(d => d.CurrentContent, x => x.MapFrom(s => s.ContentItem.Content))
                .ForMember(d => d.ContentItemId, x => x.MapFrom(s => s.ContentItem.ContentItemId))
                .ForMember(d => d.DisplayText, x => x.MapFrom(s => s.ContentItem.DisplayText));

            CreateMap<Processing, RelatedContentData>()
                .ForMember(d => d.Author, x => x.MapFrom(s => s.Author))
                .ForMember(d => d.ContentType, x => x.MapFrom(s => s.ContentType))
                .ForMember(d => d.DisplayText, x => x.MapFrom(s => s.DisplayText))
                .ForMember(d => d.ContentItemId, x => x.MapFrom(s => s.ContentItemId))
                .ForMember(d => d.FullPageUrl, option => option.Ignore())
                .ForMember(d => d.GraphSyncId, option => option.Ignore());
        }
    }
}
