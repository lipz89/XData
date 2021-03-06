﻿using System;
using System.Collections.Generic;

using XData.Extentions;

namespace XData.Meta
{
    internal static class DbTypes
    {
        static DbTypes()
        {
            AddType<bool>();
            AddType<byte>();
            AddType<sbyte>();
            AddType<short>();
            AddType<ushort>();
            AddType<char>();
            AddType<int>();
            AddType<uint>();
            AddType<long>();
            AddType<ulong>();
            AddType<float>();
            AddType<double>();
            AddType<decimal>();
            AddType<string>();
            AddType<object>();
            AddType<DateTime>();
            AddType<DateTimeOffset>();
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