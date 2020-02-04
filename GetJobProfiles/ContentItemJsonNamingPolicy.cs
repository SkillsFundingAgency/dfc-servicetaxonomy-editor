using System.Text.Json;

namespace GetJobProfiles
{
    public class ContentItemJsonNamingPolicy : JsonNamingPolicy
    {
        private readonly string _contentType;
        public ContentItemJsonNamingPolicy(string contentType)
        {
            _contentType = contentType;
        }

        public override string ConvertName(string name) => name == "EponymousPart" ? _contentType : name;
    }
}
