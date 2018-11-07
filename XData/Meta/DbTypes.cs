using System;
using System.Collections.Generic;

using XData.Extentions;

namespace XData.Meta
{
    internal static class DbTypes
    {
        static DbTypes()
        {
            AddType<int>();
            AddType<uint>();
            AddType<long>();
            AddType<ulong>();
            AddType<short>();
            AddType<ushort>();
            AddType<bool>();
            AddType<byte>();
            AddType<sbyte>();
            AddType<decimal>();
            AddType<double>();
            AddType<float>();
            AddType<string>();
            AddType<object>();
            AddType<DateTime>();
            AddType<Guid>();
            AddType<byte[]>();
        }

        private static readonly List<Type> types = new List<Type>();

        private static void AddType<T>()
        {
            AddType(typeof(T));
        }

        private static void AddType(Type type)
        {
            if (!types.Contains(type))
            {
                types.Add(type);
            }
        }

        public static bool ContainsType(Type type)
        {
            return types.Contains(type.NonNullableType());
        }

        public static bool IsSimpleType(Type type)
        {
            return DbTypes.ContainsType(type) && type != typeof(object);
        }
    }
}