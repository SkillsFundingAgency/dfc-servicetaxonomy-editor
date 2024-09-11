using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CompUi.Models;
using DfE.NCS.Framework.Event.Model;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IEventGridHandler
    {
        ContentEventData? CreateEventMessageAsync(Processing processing);
        Task SendEventMessageAsync(Processing processing, ContentEventType eventType);
    }
}
