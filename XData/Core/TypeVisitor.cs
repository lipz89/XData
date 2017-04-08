using System;
using System.Collections.Generic;
using System.Linq;

using XData.Meta;

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
            return MapperConfig.GetTableName(type);
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
}
