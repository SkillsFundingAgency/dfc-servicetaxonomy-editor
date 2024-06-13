using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IBackgroundQueue<T>
    {
        ValueTask QueueItem(T item);

        ValueTask<T> DequeueItem(CancellationToken cancellationToken);
    }
}
