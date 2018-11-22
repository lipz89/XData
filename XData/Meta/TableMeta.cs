using System;
using System.Collections.Generic;
using System.Reflection;

using XData.Common;

namespace XData.Meta
{
    internal class TableMeta
    {
        #region Constructors

        private TableMeta(Type type)
        {
            this.Type = type;
            this.TableName = MapperConfig.GetTableName(type);
        }

        #endregion

        #region Properties

        public Type Type { get; }

        public string TableName { get; }

        public IReadOnlyList<ColumnMeta> Columns { get; private set; }

        #endregion

        #region Methods

        public bool IsSimpleType()
        {
            return DbTypes.ContainsType(Type) && Type != typeof(object);
        }

        #endregion

        #region Override Object Methods

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is TableMeta)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Type.AssemblyQualifiedName.GetHashCode() ^ Constans.HashCodeXOr;
        }

        public override string ToString()
        {
            return $"TableName:{TableName}, Type:{Type.FullName}";
        }

        #endregion

        #region Static Methods

        private static readonly Cache<Type, TableMeta> cache = new Cache<Type, TableMeta>();

        public static TableMeta From<T>()
        {
            return From(typeof(T));
        }

        public static TableMeta From(Type type)
        {
            return cache.Get(type, () =>
            {
                var meta = new TableMeta(type);
                if (!meta.IsSimpleType())
                {
                    var columns = new List<ColumnMeta>();

                    foreach (var info in meta.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (DbTypes.ContainsType(info.PropertyType) && !MapperConfig.IsIgnore(info, meta.Type))
                        {
                            columns.Add(new ColumnMeta(info, meta.Type));
                        }
                    }
                    foreach (var info in meta.Type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (DbTypes.ContainsType(info.FieldType) && !MapperConfig.IsIgnore(info, meta.Type))
                        {
                            columns.Add(new ColumnMeta(info, meta.Type));
                        }
                    }
                    meta.Columns = columns.AsReadOnly();
                }
                return meta;
            });
        }

        #endregion

    }
}