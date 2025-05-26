using System.Collections.Generic;

namespace CH.CleanArchitecture.Core.Application.Workflow
{
    public class WorkflowContext
    {
        private readonly Dictionary<string, object> _data = new();

        public void Set<T>(string key, T value) => _data[key] = value;

        public T Get<T>(string key) {
            if (_data.TryGetValue(key, out var value))
                return (T)value;

            return default;
        }

        public bool TryGet<T>(string key, out T value) {
            if (_data.TryGetValue(key, out var obj) && obj is T t) {
                value = t;
                return true;
            }
            value = default;
            return false;
        }
    }
}
