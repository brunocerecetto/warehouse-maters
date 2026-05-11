using System.Collections.Generic;

namespace WM.Core
{
    public sealed class NullSaveService : ISaveService
    {
        private readonly Dictionary<string, string> _store = new Dictionary<string, string>();

        public void Save(string key, string json) => _store[key] = json;

        public string Load(string key) => _store.TryGetValue(key, out var v) ? v : null;

        public bool HasKey(string key) => _store.ContainsKey(key);
    }
}
