﻿using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class ValidateAndRepairResult
    {
        public string GraphReplicaSetName { get; }
        public int GraphInstance { get; }

        public List<ContentItem> Validated { get; } = new List<ContentItem>();
        public List<ValidationFailure> ValidationFailures { get; } = new List<ValidationFailure>();
        public List<ContentItem> Repaired { get; } = new List<ContentItem>();
        public List<RepairFailure> RepairFailures { get; } = new List<RepairFailure>();

        public ValidateAndRepairResult(string graphReplicaSetName, int graphInstance)
        {
            GraphReplicaSetName = graphReplicaSetName;
            GraphInstance = graphInstance;
        }
    }
}
