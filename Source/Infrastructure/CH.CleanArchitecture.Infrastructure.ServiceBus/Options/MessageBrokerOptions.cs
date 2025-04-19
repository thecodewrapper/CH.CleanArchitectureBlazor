using System.Reflection;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    public class MessageBrokerOptions
    {
        private readonly List<Type> _producers = new();
        private readonly List<Type> _consumers = new();
        private readonly List<Assembly> _assemblies = new();

        public string Provider { get; }
        public string HostUrl { get; }

        public MessageBrokerOptions(string provider, string hostUrl) {
            Provider = provider;
            HostUrl = hostUrl;
        }

        public MessageBrokerOptions WithProduce(params Type[] types) {
            _producers.AddRange(types ?? []);
            return this;
        }

        public MessageBrokerOptions WithConsume(params Type[] types) {
            _consumers.AddRange(types ?? []);
            return this;
        }

        public MessageBrokerOptions WithAssemblies(params Assembly[] assemblies) {
            _assemblies.AddRange(assemblies ?? []);
            return this;
        }

        internal List<Type> GetProducers() => _producers;
        internal List<Type> GetConsumers() => _consumers;
        internal List<Assembly> GetAssemblies() => _assemblies;
    }
}
