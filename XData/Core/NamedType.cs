﻿using System;
using System.Reflection;

using XData.Common;
using XData.XBuilder;

namespace XData.Core
{
    internal class NamedType
    {
        private string defaultSql;
        private Cache<MemberInfo, string> cache = new Cache<MemberInfo, string>();
        public NamedType(Type type, string name)
        {
            Type = type;
            Name = name;
        }
        public Type Type { get; }
        public string Name { get; }
        public override int GetHashCode()
        {
            return this.Type.MetadataToken ^ this.Name.GetHashCode() ^ Constans.HashCodeXOr;
        }

        public override bool Equals(object obj)
        {
            if (obj is NamedType)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        public void Add(MemberInfo member, string sql)
        {
            this.cache.AddOrReplace(member, sql);
        }
        public void AddDefault(string sql)
        {
            defaultSql = sql;
        }

        public string GetSql(MemberInfo member)
        {
            if (member == null)
            {
                return defaultSql;
            }
            if (cache.ContainsKey(member))
            {
                return cache[member];
            }
            return null;
        }

        public string GetSql(MemberInfo member, SqlBuilber builber)
        {
            if (member != null)
            {
                return cache.Get(member, () =>
                                 {
                                     return builber.EscapeSqlIdentifier(this.Name) + "." + builber.EscapeSqlIdentifier(member.Name);
                                 });
            }
            return defaultSql;
        }
    }
}