using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CH.CleanArchitecture.Infrastructure.Extensions
{
    internal static class PropertyBuilderExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter(allowIntegerValues: true) }
        };

        internal static PropertyBuilder<T> AsJsonProperty<T>(this PropertyBuilder<T> propertyBuilder) {
            var jsonConverter = new ValueConverter<T, string>(
                v => SerializeToJson(v),
                v => DeserializeFromJson<T>(v));

            var jsonComparer = new ValueComparer<T>(
                (c1, c2) => ComputeJsonHash(c1) == ComputeJsonHash(c2),
                v => v == null ? 0 : ComputeJsonHash(v),
                v => v == null ? default : DeserializeFromJson<T>(SerializeToJson(v)));

            propertyBuilder
                .HasColumnType("nvarchar(max)")
                .HasConversion(jsonConverter)
                .Metadata.SetValueComparer(jsonComparer);

            return propertyBuilder;
        }

        private static string SerializeToJson<T>(T value) {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        private static T DeserializeFromJson<T>(string json) {
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }

        private static int ComputeJsonHash<T>(T value) {
            string json = JsonSerializer.Serialize(value, _jsonSerializerOptions);
            return json.GetHashCode();
        }
    }
}
