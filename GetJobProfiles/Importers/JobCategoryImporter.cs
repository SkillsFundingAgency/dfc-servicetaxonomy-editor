using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;
using NPOI.XSSF.UserModel;
using OrchardCore.Entities;

namespace GetJobProfiles.Importers
{
    public class JobCategoryImporter
    {
        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();

        public IEnumerable<JobCategoryContentItem> JobCategoryContentItems { get; private set; }

        public Dictionary<string, string> JobCategoryContentItemIdDictionary { get; private set; }

        public void Import(XSSFWorkbook workbook, string timestamp, IEnumerable<JobProfileContentItem> jobProfiles)
        {
            var sheet = workbook.GetSheet("JobProfile");
            var categoriesColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "JobProfileCategories")
                .ColumnIndex;
            var uriColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ItemDefaultUrl").ColumnIndex;

            var jobCategoryDictionary = new Dictionary<string, string[]>();

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                var categories = row.GetCell(categoriesColumnIndex).StringCellValue.Split(",");
                var uri = row.GetCell(uriColumnIndex).StringCellValue.TrimStart('/');

                jobCategoryDictionary.Add(uri, categories);
            }

            JobCategoryContentItemIdDictionary = jobCategoryDictionary.SelectMany(z => z.Value).Distinct().Select(jc => new { Id = _generator.GenerateUniqueId(), Title = jc.Trim() }).ToDictionary(y => y.Title, y => y.Id);

            JobCategoryContentItems = jobCategoryDictionary.SelectMany(dict => dict.Value).Distinct().ToList().Select(z => z.Trim()).Select(category
                 => new JobCategoryContentItem(category, timestamp, JobCategoryContentItemIdDictionary[category])
                 {
                     EponymousPart = new JobCategoryPart
                     {
                         Description = new HtmlField(category),
                         JobProfiles = new ContentPicker
                         {
                             ContentItemIds = jobProfiles
                                .Where(jp =>
                                    jobCategoryDictionary.Where(dict => dict.Value.Any(val => val == category))
                                        .Select(dict => dict.Key).Any(uri =>
                                            jp.JobProfileHeader.JobProfileWebsiteUrl.Text.EndsWith(uri)))
                                .Select(jp => jp.ContentItemId)
                         },
                         WebsiteURI = new TextField($"/job-categories/{category.ToLower().Replace(' ', '-')}")
                     }
                 });
        }
    }
}
