namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IBackgroundQueue<T>
    {
        ValueTask QueueItem(T item);

        ValueTask<T> DequeueItem(CancellationToken cancellationToken);
    }
}
