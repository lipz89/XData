using System;
using System.Collections.Generic;

namespace XData.Common
{
    internal class Cache<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public TValue Get(TKey key, Func<TValue> creator)
        {
            if (!this.ContainsKey(key))
            {
                var value = creator.Invoke();
                this.Add(key, value);
            }
            return this[key];
        }
    }
}