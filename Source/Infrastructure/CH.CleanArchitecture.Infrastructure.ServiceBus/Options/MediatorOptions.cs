using System.Reflection;

namespace CH.CleanArchitecture.Infrastructure.ServiceBus
{
    public class MediatorOptions
    {
        private readonly List<Type> _consumers = new();
        private readonly List<Assembly> _assemblies = new();

        public MediatorOptions WithConsumers(params Type[] types) {
            _consumers.AddRange(types ?? []);
            return this;
        }

        public MediatorOptions WithAssemblies(params Assembly[] assemblies) {
            _assemblies.AddRange(assemblies ?? []);
            return this;
        }

        internal List<Type> GetConsumers() => _consumers;
        internal List<Assembly> GetAssemblies() => _assemblies;
    }
}
