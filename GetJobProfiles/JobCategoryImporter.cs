using System.Collections.Generic;
using System.IO;
using System.Linq;
using GetJobProfiles.Models.Recipe;
using NPOI.XSSF.UserModel;

namespace GetJobProfiles
{
    public class JobCategoryImporter
    {
        public IEnumerable<JobCategoryContentItem> JobCategoryContentItems { get; private set; }

        public void Import(string timestamp, IEnumerable<JobProfileContentItem> jobProfiles)
        {
            using (var reader = new StreamReader(@"SeedData\job_profiles.xlsx"))
            {
                var workbook = new XSSFWorkbook(reader.BaseStream);
                var sheet = workbook.GetSheet("JobProfile");
                var categoriesColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "JobProfileCategories").ColumnIndex;
                var uriColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ItemDefaultUrl").ColumnIndex;

                var jobCategoryDictionary = new Dictionary<string, string[]>();

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    var categories = row.GetCell(categoriesColumnIndex).StringCellValue.Split(",");
                    var uri = row.GetCell(uriColumnIndex).StringCellValue;

                    jobCategoryDictionary.Add(uri, categories);
                }

                JobCategoryContentItems = jobCategoryDictionary.SelectMany(dict => dict.Value).Distinct().ToList().Select(category => new JobCategoryContentItem(category, timestamp)
                {
                    EponymousPart = new JobCategoryPart
                    {
                        Description = new HtmlField(category),
                        JobProfiles = new ContentPicker
                        {
                            ContentItemIds = jobProfiles
                                .Where(jp => jobCategoryDictionary.Where(dict => dict.Value.Any(val => val == category)).Select(dict => dict.Key).Any(uri => jp.EponymousPart.JobProfileWebsiteUrl.Text.EndsWith(uri)))
                                .Select(jp => jp.ContentItemId)
                        }
                    }
                });
            }
        }
    }
}
