using System.Collections.Generic;

namespace GetJobProfiles.Models.Recipe.Fields.Factories
{
    public class SocCodeContentPickerFactory
    {
        private readonly Dictionary<string, string> _socCodeDictionary;

        public SocCodeContentPickerFactory(Dictionary<string, string> socCodeDictionary)
        {
            _socCodeDictionary = socCodeDictionary;
        }

        public ContentPicker Create(string socCode)
        {
            if (!_socCodeDictionary.TryGetValue(socCode, out string contentItemId))
                contentItemId = _socCodeDictionary[SocCodeConverter.UnknownSocCode];

            return new ContentPicker {ContentItemIds = new[] {contentItemId}};
        }
    }
}
