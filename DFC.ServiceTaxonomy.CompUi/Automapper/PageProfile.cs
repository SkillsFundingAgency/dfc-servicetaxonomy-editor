using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using DFC.ServiceTaxonomy.CompUi.Models;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.CompUi.Automapper
{
    [ExcludeFromCodeCoverage]
    public class PageProfile : Profile
    {
        public PageProfile()
        {
            CreateMap<ContentContextBase, Processing>()
            .ForMember(d => d.ContentItemId, x => x.MapFrom(s => s.ContentItem.Id))
            .ForMember(d => d.ContentType, x => x.MapFrom(s => s.ContentItem.ContentType))
            .ForMember(d => d.Latest, x => x.MapFrom(s => Convert.ToInt32(s.ContentItem.Latest)))
            .ForMember(d => d.Published, x => x.MapFrom(s => Convert.ToInt32(s.ContentItem.Published))); 
        }
    }
}
