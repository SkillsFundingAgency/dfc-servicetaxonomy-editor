using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CompUi.Models;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IJobProfileCacheRefresh
    {
        Task RefreshAllJobProfileContent(Processing processing);
    }
}
