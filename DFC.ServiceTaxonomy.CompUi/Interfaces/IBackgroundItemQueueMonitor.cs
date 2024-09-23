using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IBackgroundItemQueueMonitor
    {
        void TryStart(CancellationToken cancellationToken);
    }
}
