using System.Collections.Generic;
using System.Linq;
using GetJobProfiles.Models.Containers;
using GetJobProfiles.Models.Recipe.ContentItems;
using GetJobProfiles.Models.Recipe.Fields;
using GetJobProfiles.Models.Recipe.Parts;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OrchardCore.Entities;

namespace GetJobProfiles.Builders
{
    public class JobCategoryBuilder
    {
        public Dictionary<string, string> JobCategoryContentItemIdDictionary { get; private set; }

        public IEnumerable<JobCategoryContentItem> JobCategoryContentItems { get; private set; }

        private static readonly DefaultIdGenerator _generator = new DefaultIdGenerator();
        private readonly JobProfileSettingsDataModel _jobProfileSettingsDataModel;
        private readonly ReferenceData _jobProfileReferenceDataModel;

        public JobCategoryBuilder(
            JobProfileSettingsDataModel jobProfileSettingsDataModel,
            ReferenceData jobProfileReferenceDataModel)
        {
            _jobProfileSettingsDataModel = jobProfileSettingsDataModel;
            _jobProfileReferenceDataModel = jobProfileReferenceDataModel;
        }

        public JobCategoryBuilder Build()
        {
            var sheet = _jobProfileReferenceDataModel.JobProfileExcelWorkbook.GetSheet("JobProfile");
            var categoriesColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "JobProfileCategories")
                .ColumnIndex;
            var uriColumnIndex = sheet.GetRow(0).Cells.Single(x => x.StringCellValue == "ItemDefaultUrl").ColumnIndex;

            var jobCategoryDictionary = new Dictionary<string, string[]>();

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                //multiple categories are split using a comma without a space
                //if there is a space after the comma it should be part of the string so for now replace it with something random
                string categoriesCommaSeparatedString = row.GetCell(categoriesColumnIndex).StringCellValue.Replace(", ", "|||");
                //now we can split on the comma to actually get the proper categories list
                string[] categoriesList = categoriesCommaSeparatedString.Split(',');
                //now go back and put the comma+space back where it belongs
                categoriesList = categoriesList.Select(x => x.Replace("|||", ", ")).ToArray();

                string uri = row.GetCell(uriColumnIndex).StringCellValue.TrimStart('/');

                jobCategoryDictionary.Add(uri, categoriesList);
            }

            JobCategoryContentItemIdDictionary = jobCategoryDictionary.SelectMany(z=>z.Value).Distinct().Select(jc => new { Id = _generator.GenerateUniqueId(), Title = jc}).ToDictionary(y => y.Title, y => y.Id);

            JobCategoryContentItems = jobCategoryDictionary.SelectMany(dict => dict.Value).Distinct().ToList().Select(
                category => new JobCategoryContentItem(category, _jobProfileSettingsDataModel.Timestamp, JobCategoryContentItemIdDictionary[category])
                {
                    PageLocationPart = new PageLocationPart
                    {
                        UrlName = category.ToLower().Replace(' ', '-'),
                        FullUrl = $"/job-categories/{category.ToLower().Replace(' ', '-')}"
                    },
                    EponymousPart = new JobCategoryPart
                    {
                        PageLocations = new TaxonomyField
                        {
                            TaxonomyContentItemId = "4eembshqzx66drajtdten34tc8",
                            TermContentItemIds = new[] { "4y9c37rra2k0x6dhqg796akwxk" }
                        },
                        Description = new HtmlField(category),
                        JobProfiles = new ContentPicker
                        {
                            ContentItemIds = _jobProfileReferenceDataModel.JobProfileContentItemBuilder.jobProfileContentItems
                                .Where(jp =>
                                    jobCategoryDictionary.Where(dict => dict.Value.Any(val => val == category))
                                        .Select(dict => dict.Key).Any(uri =>
                                            jp.PageLocationPart.FullUrl.EndsWith(uri)))
                                .Select(jp => jp.ContentItemId)
                        }
                    }
                });

            return this;
        }
    }
}
