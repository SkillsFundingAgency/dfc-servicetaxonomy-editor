using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.Automapper
{
    [ExcludeFromCodeCoverage]
    public class PageProfile : Profile
    {
        public PageProfile()
        {
            //CreateMap<NodeItem, Page>()
            //.ForMember(d=>d.FullUrl, s);
        }
    }
}
