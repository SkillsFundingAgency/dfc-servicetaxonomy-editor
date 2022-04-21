using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Services;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.DataSync.Notifications
{
    public interface IDataSyncNotifier : INotifier
    {
        Task AddBlocked(
            SyncOperation syncOperation,
            ContentItem contentItem,
            IEnumerable<(string GraphReplicaSetName, IAllowSync AllowSync)> graphBlockers);

        Task Add(
            string userMessage,
            string technicalMessage = "",
            Exception? exception = null,
            HtmlString? technicalHtmlMessage = null,
            HtmlString? userHtmlMessage = null,
            NotifyType type = NotifyType.Error);
    }
}
