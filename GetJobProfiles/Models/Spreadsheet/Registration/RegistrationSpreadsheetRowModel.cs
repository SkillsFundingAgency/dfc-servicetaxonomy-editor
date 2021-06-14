﻿using System;

namespace GetJobProfiles.Models.Spreadsheet.Registration
{
    public class RegistrationSpreadsheetRowModel
    {
        // Properties for the Sitefinity Registration speadsheet on the exported Job Profile workbook

        // SystemParentId
        public string SystemParentId { get; set; }

        // IncludeInSitemap
        public bool? IncludeInSitemap { get; set; }

        // Id
        public string Id { get; set; }

        // DateCreated
        public DateTime? DateCreated { get; set; }

        // ItemDefaultUrl
        public string ItemDefaultUrl { get; set; }

        // UrlName
        public string UrlName { get; set; }

        // PublicationDate
        public DateTime? PublicationDate { get; set; }

        // Info
        public string Info { get; set; }

        // Title
        public string Title { get; set; }
    }
}
