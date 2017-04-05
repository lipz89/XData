using System;
using System.Collections.Generic;

using XData.Common;
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

        private static readonly List<Type> Types = new List<Type>();

        public static void AddType<T>()
        {
            AddType(typeof(T));
        }

        public static void AddType(Type type)
        {
            if (!Types.Contains(type))
            {
                Types.Add(type);
            }
        }

        public static bool ContainsType(Type type)
        {
            return Types.Contains(type.NonNullableType());
        }
        public static bool IsSimpleType(Type type)
        {
            return DbTypes.ContainsType(type) && type != typeof(object);
        }
    }
}