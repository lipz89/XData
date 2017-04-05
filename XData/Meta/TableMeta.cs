using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

using XData.Common;
using XData.Extentions;
using XData.XBuilder;

namespace XData.Meta
{
    [DebuggerDisplay("TableName:{TableName}, Type:{Type.FullName}")]
    internal class TableMeta
    {
        public TableMeta(Type type)
        {
            this.Type = type;
            this.TableName = MetaConfig.GetTableName(type);
        }
        public Type Type { get; }
        public string TableName { get; internal set; }
        public ColumnMeta Key { get; internal set; }
        public IReadOnlyList<ColumnMeta> Columns { get; internal set; }

        public bool IsSimpleType()
        {
            return DbTypes.ContainsType(Type) && Type != typeof(object);
        }
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
            return this.Type.GetHashCode() ^ Constans.HashCodeXOr;
        }

        public static TableMeta From<T>(string tableName = null)
        {
            if (Inner<T>.Meta == null)
            {
                if (!tableName.IsNullOrWhiteSpace())
                {
                    MetaConfig.MetaTableName<T>(tableName);
                }
                var meta = new TableMeta(typeof(T));
                if (!meta.IsSimpleType())
                {
                    meta.Key = MetaConfig.GetKeyMeta<T>();

                    var columns = new List<ColumnMeta>();

                    foreach (var info in meta.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        var mi = new MInfo(info, meta.Type);
                        if (DbTypes.ContainsType(info.PropertyType) && !MetaConfig.IsIgnore(info, meta.Type))
                        {
                            var columnName = MetaConfig.GetColumnName(info, meta.Type);
                            columns.Add(new ColumnMeta(mi)
                            {
                                ColumnName = columnName
                            });
                        }
                    }
                    foreach (var info in meta.Type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                    {
                        var mi = new MInfo(info, meta.Type);
                        if (DbTypes.ContainsType(info.FieldType) && !MetaConfig.IsIgnore(info, meta.Type))
                        {
                            var columnName = MetaConfig.GetColumnName(info, meta.Type);
                            columns.Add(new ColumnMeta(mi)
                            {
                                ColumnName = columnName
                            });
                        }
                    }
                    meta.Columns = columns.AsReadOnly();
                }
                Inner<T>.Meta = meta;
            }
            return Inner<T>.Meta;
        }

        private class Inner<T>
        {
            internal static TableMeta Meta { get; set; }
        }
    }
}