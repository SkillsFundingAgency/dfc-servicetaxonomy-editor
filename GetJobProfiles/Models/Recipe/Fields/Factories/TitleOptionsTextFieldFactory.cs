﻿using System.Collections.Generic;

namespace GetJobProfiles.Models.Recipe.Fields.Factories
{
    public class TitleOptionsTextFieldFactory
    {
        private readonly Dictionary<string, string> _titleOptionsLookup;

        public TitleOptionsTextFieldFactory(Dictionary<string, string> titleOptionsLookup)
        {
            _titleOptionsLookup = titleOptionsLookup;
        }

        public TextField Create(string url)
        {
            //todo: need to create a report
            if (!_titleOptionsLookup.TryGetValue(url, out string titleOption))
            {
                titleOption = "as_defined";
            }

            return new TextField(titleOption);
        }
    }
}
