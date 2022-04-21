using System;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers
{
    public class DisposeAction : IDisposable
    {
        private readonly Action _cleanUp;

        public DisposeAction(Action cleanUp)
        {
            _cleanUp = cleanUp;
        }

        public void Dispose()
        {
            _cleanUp();
        }
    }
}
