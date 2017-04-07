using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using XData.Common;
using XData.Meta;
using XData.XBuilder;

namespace XData.Core
{
    internal class TypeVisitor
    {
        internal readonly List<NamedType> typeNames = new List<NamedType>();

        public TypeVisitor()
        {

        }
        public TypeVisitor(IEnumerable<NamedType> values)
        {
            this.typeNames.AddRange(values);
        }

        internal string GetTypeName(Type type)
        {
            var t = typeNames.FirstOrDefault(x => x.Type == type);
            if (t != null)
            {
                return t.Name;
            }
            return MetaConfig.GetTableName(type);
        }
        internal NamedType Get(Type type)
        {
            foreach (var typeName in typeNames)
            {
                if (typeName.Type == type)
                {
                    return typeName;
                }
            }
            return null;
        }
        internal bool IsEqualsTo(Type type)
        {
            foreach (var typeName in typeNames)
            {
                if (typeName.Type == type)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool IsAssignableTo(Type type, out Type outType)
        {
            foreach (var typeName in typeNames)
            {
                if (typeName.Type.IsAssignableFrom(type))
                {
                    outType = typeName.Type;
                    return true;
                }
            }
            outType = null;
            return false;
        }

        public void Add(Type type, string name)
        {
            Add(new NamedType(type, name));
        }

        public void Add(NamedType namedType)
        {
            this.typeNames.Add(namedType);
        }
    }

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
