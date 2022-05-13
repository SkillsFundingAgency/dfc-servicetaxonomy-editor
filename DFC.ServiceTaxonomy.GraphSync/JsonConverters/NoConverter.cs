using System;
using Newtonsoft.Json;

namespace DFC.ServiceTaxonomy.GraphSync.JsonConverters
{
    public class NoConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => false;

        public override bool CanRead { get { return false; } }
        public override bool CanWrite { get { return false; } }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => throw new NotImplementedException();
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
