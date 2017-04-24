using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace XData.Common
{
    /// <summary>
    /// 用字典模拟简单缓存
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class Cache<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        public TValue Get(TKey key, Func<TValue> creator)
        {
            if (!this.ContainsKey(key))
            {
                var value = creator.Invoke();
                this.TryAdd(key, value);
            }
            return this[key];
        }

        public void AddOrReplace(TKey key, TValue value)
        {
            if (this.ContainsKey(key))
            {
                this[key] = value;
            }
            else
            {
                this.TryAdd(key, value);
            }
        }
    }
}