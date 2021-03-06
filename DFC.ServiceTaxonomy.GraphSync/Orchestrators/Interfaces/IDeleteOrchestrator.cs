﻿using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Orchestrators.Interfaces
{
    public interface IDeleteOrchestrator
    {
        Task<bool> Unpublish(ContentItem contentItem);
        Task<bool> Delete(ContentItem contentItem);
    }
}
