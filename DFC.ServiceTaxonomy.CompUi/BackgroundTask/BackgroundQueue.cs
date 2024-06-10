using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CompUi.Interfaces;

namespace DFC.ServiceTaxonomy.CompUi.BackgroundTask
{
    public class BackgroundQueue<T> : IBackgroundQueue<T>
    {
        private const int Capacity = 10000;
        private readonly Channel<T> channel;

        public BackgroundQueue()
        {
            var options = new BoundedChannelOptions(Capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
            };
            channel = Channel.CreateBounded<T>(options);
        }

        public async ValueTask QueueItem(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            await channel.Writer.WriteAsync(item);
        }

        public async ValueTask<T> DequeueItem(CancellationToken cancellationToken)
        {
            var t = await channel.Reader.ReadAsync(cancellationToken);
            return t;
        }
    }
}
