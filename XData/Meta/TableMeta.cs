using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using XData.Common;
using XData.Extentions;

namespace XData.Meta
{
    [DebuggerDisplay("TableName:{TableName}, Type:{Type.FullName}")]
    internal class TableMeta
    {
        #region Constructors

        public TableMeta(Type type)
        {
            this.Type = type;
            this.TableName = MapperConfig.GetTableName(type);
        }

        #endregion

        #region Properties

        public Type Type { get; }

        public string TableName { get; internal set; }

        public ColumnMeta Key
        {
            get { return MapperConfig.GetKeyMeta(this.Type); }
        }

        public IReadOnlyList<ColumnMeta> Columns { get; internal set; }


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
            if (obj is TableMeta)
            {
                return this.GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Type.MetadataToken ^ Constans.HashCodeXOr;
        }

        #endregion

        #region Static Methods

        private static readonly Cache<Type, TableMeta> cache = new Cache<Type, TableMeta>();

        public static TableMeta From<T>(string tableName = null)
        {
            return From(typeof(T), tableName);
        }

        public static TableMeta From(Type type, string tableName = null)
        {
            return cache.Get(type, () =>
            {
                if (!tableName.IsNullOrWhiteSpace())
                {
                    MapperConfig.HasTableName(type, tableName);
                }
                var meta = new TableMeta(type);
                if (!meta.IsSimpleType())
                {
                    var columns = new List<ColumnMeta>();

                    foreach (var info in meta.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        var mi = new MInfo(info, meta.Type);
                        if (DbTypes.ContainsType(info.PropertyType) && !MapperConfig.IsIgnore(info, meta.Type))
                        {
                            var columnName = MapperConfig.GetColumnName(info, meta.Type);
                            columns.Add(new ColumnMeta(mi)
                            {
                                ColumnName = columnName
                            });
                        }
                    }
                    foreach (var info in meta.Type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                    {
                        var mi = new MInfo(info, meta.Type);
                        if (DbTypes.ContainsType(info.FieldType) && !MapperConfig.IsIgnore(info, meta.Type))
                        {
                            var columnName = MapperConfig.GetColumnName(info, meta.Type);
                            columns.Add(new ColumnMeta(mi)
                            {
                                ColumnName = columnName
                            });
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