using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.ServiceTaxonomy.UnpublishLater.Models
{
    public static class Constants
    {
        // Content event names
        public const string ContentEvent_Published = "Published";
        public const string ContentEvent_Unpublished = "Unpublished";

        // Submit names
        public const string Submit_Publish_Key = "submit.Publish";
        public const string Submit_Unpublish_Later = "submit.UnpublishLater";
        public const string Submit_Cancel_Unpublish_Later = "submit.CancelUnpublishLater";
    }
}
