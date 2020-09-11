using System;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public sealed class SyncOperation : IFormattable
    {
        //todo: check doesn't create one each time
        //todo: add as singletons instead?
        // public static SyncOperation Delete => new SyncOperation(Operation.Delete, "Deleting");
        // public static SyncOperation Unpublish => new SyncOperation(Operation.Unpublish, "Unpublishing");
        // public static SyncOperation SaveDraft => new SyncOperation(Operation.SaveDraft, "Saving a draft of");
        // public static SyncOperation Publish => new SyncOperation(Operation.Publish, "Publishing");
        // public static SyncOperation Update => new SyncOperation(Operation.Update, "Updating");
        // public static SyncOperation DiscardDraft => new SyncOperation(Operation.DiscardDraft, "Discarding the draft of");
        // public static SyncOperation Clone => new SyncOperation(Operation.Clone, "Cloning");

        // private enum Operation
        // {
        //     Delete,
        //     Unpublish,
        //     SaveDraft,
        //     Publish,
        //     Update,
        //     DiscardDraft,
        //     Clone
        // }

        //private with public properties??
        public static readonly SyncOperation Delete = new SyncOperation("Deleting", "Deleted");
        public static readonly SyncOperation Unpublish = new SyncOperation("Unpublishing", "Unpublished");
        public static readonly SyncOperation SaveDraft = new SyncOperation("Saving a draft of", "Saved a draft of");
        public static readonly SyncOperation Publish = new SyncOperation("Publishing", "Published");
        public static readonly SyncOperation Update = new SyncOperation("Updating", "Updated");
        public static readonly SyncOperation DiscardDraft = new SyncOperation("Discarding the draft of", "Discarded the draft of");
        public static readonly SyncOperation Clone = new SyncOperation("Cloning", "Cloned");

        //private readonly Operation _operation;
        private readonly string _presentContinuous;
        private readonly string _presentPerfect;

        // private SyncOperation(Operation operation, string presentContinuous)
        // {
        //     _operation = operation;
        //     _presentContinuous = presentContinuous;
        // }

        private SyncOperation(string presentContinuous, string presentPerfect)
        {
            _presentContinuous = presentContinuous;
            _presentPerfect = presentPerfect;
        }

        //don't think this is necessary: default reference equals should be ok
        // private bool Equals(SyncOperation other) => _operation == other._operation;
        //
        // public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is SyncOperation other && Equals(other);

        // public override int GetHashCode() => (int) _operation;

        //todo: do we still need the override?
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
