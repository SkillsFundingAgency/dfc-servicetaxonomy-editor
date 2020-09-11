﻿using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Services;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Notifications
{
    public interface ICustomNotifier : INotifier
    {
        void AddBlocked(
            SyncOperation syncOperation,
            ContentItem contentItem,
            IEnumerable<(string GraphReplicaSetName, IAllowSyncResult AllowSyncResult)> graphBlockers);

        void Add(
            string userMessage,
            string technicalMessage = "",
            Exception? exception = null,
            HtmlString? technicalHtmlMessage = null,
            HtmlString? userHtmlMessage = null,
            NotifyType type = NotifyType.Error);
    }
}
