using System;
using System.Collections.Generic;

namespace XData.Meta
{
    public static class XDataMapper
    {
        private static readonly IDictionary<Type, TableConfig> CachedTableConfigs = new Dictionary<Type, TableConfig>();

        public static void Config<T>(Action<TableConfig> cfg) where T : class, new()
        {
            cfg?.Invoke(GetConfig<T>());
        }

        public static TableConfig GetConfig(Type type)
        {
            lock (CachedTableConfigs)
            {
                if (!CachedTableConfigs.ContainsKey(type))
                {
                    CachedTableConfigs.Add(type, new TableConfig(type));
                }
            }

            return CachedTableConfigs[type];
        }

        public static TableConfig GetConfig<T>() where T : class, new()
        {
            return GetConfig(typeof(T));
        }
    }
}