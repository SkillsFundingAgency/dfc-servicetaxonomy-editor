using System.Collections.Generic;
using GetJobProfiles.Models.Containers;

namespace GetJobProfiles.Models.Recipe.Fields.Factories
{
    public class TitleOptionsTextFieldFactory
    {
        private readonly Dictionary<string, JobProfileExcelWorkbookColumnsDataModel> _titleOptionsDictionary;

        public TitleOptionsTextFieldFactory(Dictionary<string, JobProfileExcelWorkbookColumnsDataModel> titleOptionsDictionary)
        {
            _titleOptionsDictionary = titleOptionsDictionary;
        }

        public TextField Create(string url)
        {
            //todo: need to create a report
            if (!_titleOptionsDictionary.TryGetValue(url, out JobProfileExcelWorkbookColumnsDataModel titleOption))
            {
                titleOption = new JobProfileExcelWorkbookColumnsDataModel();
                titleOption.TitleOptions = "as_defined";
            }

            return new TextField(titleOption.TitleOptions);
        }
    }
}
