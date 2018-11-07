using System;
using System.Collections.Generic;

namespace XData.Common
{
    /// <summary>
    /// 用字典模拟简单缓存
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class Cache<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        public TValue Get(TKey key, Func<TValue> creator = null)
        {
            lock (dictionary)
            {
                if (!dictionary.ContainsKey(key))
                {
                    if (creator != null)
                    {
                        var value = creator.Invoke();
                        dictionary.Add(key, value);
                    }
                    else
                    {
                        return default(TValue);
                    }
                }
                return dictionary[key];
            }
        }

        public void AddOrReplace(TKey key, TValue value)
        {
            lock (dictionary)
            {
                if (dictionary.ContainsKey(key))
                {
                    dictionary[key] = value;
                }
                else
                {
                    dictionary.Add(key, value);
                }
            }
        }
    }
}