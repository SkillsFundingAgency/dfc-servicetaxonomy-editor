using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.AppRegistry
{
    public interface IPageLocationUpdater
    {
        Task<ContentPageModel?> GetPageById(string pageId, List<string> locations);

        Task<ContentPageModel> UpdatePages(string nodeId, List<string> locations);
    }
}
