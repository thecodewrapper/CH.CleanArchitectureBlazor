using System.Text.Json;
using CH.CleanArchitecture.Core.Application;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    /// <summary>
    /// Serializer for messages using System.Text.Json. You can replace this with your own serializer or have one for each message broker, depending on your needs
    /// </summary>
    internal class JsonMessageSerializer : IMessageSerializer
    {
        public string Serialize(object obj) => JsonSerializer.Serialize(obj);
        public object Deserialize(string data, Type type) => JsonSerializer.Deserialize(data, type)!;
    }
}
