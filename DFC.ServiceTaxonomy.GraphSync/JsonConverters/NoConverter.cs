using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DFC.ServiceTaxonomy.GraphSync.JsonConverters
{
    public class NoConverter : JsonConverter<object>
    {
        public NoConverter() { }

        public override bool CanConvert(Type objectType) => false;
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options) => throw new NotImplementedException();

    }
}
