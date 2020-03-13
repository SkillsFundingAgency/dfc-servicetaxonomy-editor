using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;
using System.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Notifications
{
    public class CustomNotifier : INotifier
    {
        private readonly IList<NotifyEntry> _entries;

        public CustomNotifier(ILogger<Notifier> logger)
        {
            Logger = logger;
            _entries = new List<NotifyEntry>();
        }

        public ILogger Logger { get; set; }

        public void Add(NotifyType type, LocalizedHtmlString message)
        {
            _entries.Add(new NotifyEntry { Type = type, Message = message });
        }

        public IList<NotifyEntry> List()
        {
            if (_entries.Any(x => x.Type == NotifyType.Error))
            {
                return _entries.Where(x => x.Type != NotifyType.Success).ToList();
            }

            return _entries;
        }
    }
}
