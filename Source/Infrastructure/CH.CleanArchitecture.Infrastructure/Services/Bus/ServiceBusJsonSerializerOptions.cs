using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CH.CleanArchitecture.Core.Domain.User;

namespace CH.CleanArchitecture.Infrastructure.Services
{
    internal class ServiceBusJsonSerializerOptions
    {
        public JsonSerializerOptions Options { get; }

        public ServiceBusJsonSerializerOptions() {
            Options = new JsonSerializerOptions
            {
                Converters = { new RoleEnumJsonConverter() },
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }
    }

    internal class RoleEnumJsonConverter : JsonConverter<RoleEnum>
    {
        public override RoleEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            return Enum.Parse<RoleEnum>(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, RoleEnum value, JsonSerializerOptions options) {
            writer.WriteStringValue(value.ToString());
        }
    }
}
