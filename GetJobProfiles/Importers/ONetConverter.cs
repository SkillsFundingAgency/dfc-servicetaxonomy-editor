using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Recipe.ContentItems;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles.Importers
{
    public class ONetConverter
    {
        public List<ONetOccupationalCodeContentItem> ONetOccupationalCodeContentItems { get; private set; } = new List<ONetOccupationalCodeContentItem>();
        public Dictionary<string, string> ONetOccupationalCodeToSocCodeDictionary = new Dictionary<string, string>();

        public static readonly string UnknownJobProfile = "Unknown";
        private static readonly string UnknownCode = "00-0000.00";
        private static readonly string UnknownSocCode = "9999";
        private readonly string[] _codesToProcess;
        private readonly bool _processAll;

        public ONetConverter(string[] codes)
        {
            _codesToProcess = codes;
            _processAll = !_codesToProcess.Any();
        }

        public Dictionary<string, string> Go(XSSFWorkbook workbook, string timestamp)
        {
            string contentItemId;

            var sheet = workbook.GetSheet("JobProfileSoc");
            var occupationalCodeColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ONetOccupationalCode").ColumnIndex;
            var jobProfileColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "Description").ColumnIndex;
            var socCodeColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "SOCCode").ColumnIndex;

            var dict = new Dictionary<string, string>();

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                var occupationalCode = row.GetCell(occupationalCodeColumnIndex).StringCellValue ?? UnknownCode;
                var socCode = row.GetCell(socCodeColumnIndex).StringCellValue ?? UnknownSocCode;

                if (_processAll || _codesToProcess.Contains(occupationalCode)) //|| occupationalCode == UnknownCode)
                {
                    var jobProfile = row.GetCell(jobProfileColumnIndex).StringCellValue;
                    var existingContentItem = ONetOccupationalCodeContentItems.SingleOrDefault(s => s.DisplayText == occupationalCode);

                    if (existingContentItem == null)
                    {
                        if (!ONetOccupationalCodeToSocCodeDictionary.ContainsKey(socCode))
                        {
                            ONetOccupationalCodeToSocCodeDictionary.Add(socCode, occupationalCode);
                        }

                        var contentItem = new ONetOccupationalCodeContentItem(occupationalCode, timestamp);
                        ONetOccupationalCodeContentItems.Add(contentItem);
                        contentItemId = contentItem.ContentItemId;
                    }
                    else
                    {
                        contentItemId = existingContentItem.ContentItemId;
                    }
                    dict.Add(jobProfile, contentItemId);
                }
            }
            // add placeholder / unknown item
            var unknownContentItem = new ONetOccupationalCodeContentItem(UnknownCode, timestamp);
            ONetOccupationalCodeContentItems.Add(unknownContentItem);
            dict.Add(UnknownJobProfile, unknownContentItem.ContentItemId);

            return dict;
        }
    }
}
