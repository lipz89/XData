using System;
using System.Collections.Generic;

namespace XData.Common
{
    /// <summary>
    /// 用字典模拟简单缓存
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
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

        public void AddOrReplace(TKey key, TValue value)
        {
            if (this.ContainsKey(key))
            {
                this[key] = value;
            }
            else
            {
                this.Add(key, value);
            }
        }
    }
}