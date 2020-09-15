using System;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public sealed class SyncOperation : IFormattable
    {
        //todo: private with public properties??
        public static readonly SyncOperation Delete = new SyncOperation("Deleting", "Deleted");
        public static readonly SyncOperation Unpublish = new SyncOperation("Unpublishing", "Unpublished");
        public static readonly SyncOperation SaveDraft = new SyncOperation("Saving a draft of", "Saved a draft of");
        public static readonly SyncOperation Publish = new SyncOperation("Publishing", "Published");
        public static readonly SyncOperation Update = new SyncOperation("Updating", "Updated");
        public static readonly SyncOperation DiscardDraft = new SyncOperation("Discarding the draft of", "Discarded the draft of");
        public static readonly SyncOperation Clone = new SyncOperation("Cloning", "Cloned");

        private readonly string _presentContinuous;
        private readonly string _presentPerfect;

        private SyncOperation(string presentContinuous, string presentPerfect)
        {
            _presentContinuous = presentContinuous;
            _presentPerfect = presentPerfect;
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            format ??= "G";

            return format switch
            {
                "G" => _presentContinuous,
                "PrP" => _presentPerfect,
                _ => throw new ArgumentException($"Unknown {nameof(format)} '{format}'.")
            };
        }
    }
}
