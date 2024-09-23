namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IBackgroundItemQueueMonitor
    {
        void TryStart(CancellationToken cancellationToken);
    }
}
