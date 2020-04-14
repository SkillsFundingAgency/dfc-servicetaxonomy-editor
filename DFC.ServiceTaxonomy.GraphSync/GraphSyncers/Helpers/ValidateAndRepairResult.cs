using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class ValidationFailure
    {
        public ContentItem ContentItem { get; set; }
        public string Reason { get; set; }

        public ValidationFailure(ContentItem contentItem, string reason)
        {
            ContentItem = contentItem;
            Reason = reason;
        }
    }

    public class RepairFailure
    {
        public ContentItem ContentItem { get; set; }
        public string Reason { get; set; }

        public RepairFailure(ContentItem contentItem, string reason)
        {
            ContentItem = contentItem;
            Reason = reason;
        }
    }

    public class ValidateAndRepairResult
    {
        // might be better to calc this from the lists instead?
        // public bool AllNewOrModifiedContentItemsValidatedOrRepaired
        // {
        //     get
        //     {
        //         return !RepairFailures.Any();
        //     }
        // }

        public DateTime LastSync { get; }

        public List<ValidationFailure> ValidationFailures { get; } = new List<ValidationFailure>();
        public List<ContentItem> Repaired { get; } = new List<ContentItem>();
        public List<RepairFailure> RepairFailures { get; } = new List<RepairFailure>();

        public ValidateAndRepairResult(DateTime lastSync) => LastSync = lastSync;

        // public ValidateAndRepairResult()
        // {
        //     AllNewOrModifiedContentItemsValidatedOrRepaired = true;
        // }

        // public void AddValidationFailure(ContentItem contentItem, string reason)
        // {
        //     ValidationFailures.Add(new ValidationFailure(contentItem, reason));
        //     AllNewOrModifiedContentItemsValidatedOrRepaired = false;
        // }
    }
}
