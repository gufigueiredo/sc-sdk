using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public sealed class PolicyContainer : IPolicyContainer
    {
        private readonly IDictionary<string, IsPolicy> _registry = new ConcurrentDictionary<string, IsPolicy>();
       
        public void Add<T>(string key, T value)
            where T : IsPolicy
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var registry = _registry as ConcurrentDictionary<string, IsPolicy>;

            registry.TryAdd(key, value);
        }

        public IsPolicy Get(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_registry.TryGetValue(key, out var result))
                return result;
            else
                return default;
        }

        public TPolicy Get<TPolicy>(string key) 
            where TPolicy : IsPolicy
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_registry.TryGetValue(key, out IsPolicy result))
                return (TPolicy)result;
            else
                return default;
        }
    }

    public interface IPolicyContainer
    {
        void Add<T>(string key, T value) where T : IsPolicy;
        IsPolicy Get(string key);
        TPolicy Get<TPolicy>(string key) where TPolicy : IsPolicy;
    }
}
