using System;

namespace GetJobProfiles.Models.Spreadsheet.DysacShortQuestion
{
    public class DysacShortQuestionSpreadsheetRowModel
    {
        // Properties for the ShortQuestion spreadsheet which is part of the Dysac workbook

        // PublicationDate
        public DateTime? PublicationDate { get; set; }

        // IsNegative
        public bool? IsNegative { get; set; }

        // QuestionText
        public string QuestionText { get; set; }

        // Trait
        public string Trait { get; set; }
    }
}
