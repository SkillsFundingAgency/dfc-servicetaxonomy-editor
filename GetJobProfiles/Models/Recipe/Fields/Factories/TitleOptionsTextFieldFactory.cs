using System.Collections.Generic;

namespace GetJobProfiles.Models.Recipe.Fields.Factories
{
    public class TitleOptionsTextFieldFactory
    {
        private readonly Dictionary<string, string> _titleOptionsLookup;

        public TitleOptionsTextFieldFactory(Dictionary<string, string> titleOptionsLookup)
        {
            _titleOptionsLookup = titleOptionsLookup;
        }

        public TextField Create(string title)
        {
            return new TextField(_titleOptionsLookup[title]);
        }
    }
}
