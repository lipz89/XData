using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
using XData.Core;
using XData.Extentions;
using XData.Meta;

namespace XData.XBuilder
{
    /// <summary>
    /// 插入命令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Insert<T> : SqlBuilber, IExecutable
    {
        #region Fields
        private readonly Strings fieldString = new Strings();
        private readonly Strings valueString = new Strings();
        private readonly ColumnMeta keyMeta;
        #endregion

        #region Constuctors

        private Insert(XContext context) : base(context)
        {
            this.tableMeta = TableMeta.From<T>();
            this.keyMeta = tableMeta.Key;
            this.tableName = this.tableMeta.TableName;
            this.namedType = new NamedType(this.tableMeta.Type, this.tableName);
            this.typeVisitor.Add(this.namedType);
        }

        /// <summary>
        /// 根据指定的实体构造一个插入命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        internal Insert(XContext context, T entity)
            : this(context)
        {
            if (entity == null)
            {
                throw Error.ArgumentNullException(nameof(entity));
            }

            var columns = tableMeta.Columns.Where(x => x.Member != keyMeta?.Member).ToList();
            foreach (var column in columns)
            {
                fieldString.Add(EscapeSqlIdentifier(column.ColumnName));
                valueString.Add(GetParameterIndex());
                this._parameters.Add(column.GetValue(entity));
            }
        }

        /// <summary>
        /// 根据实体和指定的字段构造一个插入命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="include">包含或者排除，true包含表示仅插入指定的字段，false排除表示不插入指定的字段</param>
        /// <param name="fields"></param>
        internal Insert(XContext context, T entity, bool include = false, params Expression<Func<T, object>>[] fields)
            : this(context)
        {
            if (entity == null)
            {
                throw Error.ArgumentNullException(nameof(entity));
            }
            if (fields.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(fields));
            }
            if (fields.Any(x => x == null))
            {
                throw Error.ArgumentException("指定的表达式中包含空元素。", nameof(fields));
            }
            if (fields.Any(x => x.GetMember() == null))
            {
                throw Error.ArgumentException("指定的表达式中包含不是成员访问类型的元素。", nameof(fields));
            }

            var exceptFields = fields.Select(x => x.GetPropertyName());
            var columns = tableMeta.Columns.Where(x => x.Member != keyMeta?.Member && exceptFields.Contains(x.Member.Name) == include).ToList();
            //if (columns.IsNullOrEmpty())
            //{
            //    throw Error.ArgumentException("必须插入至少一个字段。", nameof(fields));
            //}
            foreach (var column in columns)
            {
                fieldString.Add(EscapeSqlIdentifier(column.ColumnName));
                valueString.Add(GetParameterIndex());
                this._parameters.Add(column.GetValue(entity));
            }
        }
        /// <summary>
        /// 根据指定的字段和值构造一个插入命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fieldValues"></param>
        internal Insert(XContext context, IDictionary<string, object> fieldValues)
            : this(context)
        {
            if (fieldValues.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(fieldValues));
            }

            var columns = tableMeta.Columns.Where(x => x.Member != keyMeta?.Member && fieldValues.Keys.Contains(x.Member.Name)).ToList();
            //if (columns.IsNullOrEmpty())
            //{
            //    throw Error.Exception("必须插入至少一个字段。");
            //}
            foreach (var column in columns)
            {
                fieldString.Add(EscapeSqlIdentifier(column.ColumnName));
                valueString.Add(GetParameterIndex());
                this._parameters.Add(fieldValues[column.Member.Name]);
            }
        }
        #endregion

        #region IExecutable
        /// <summary>
        /// 执行插入操作
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            return Context.ExecuteNonQuery(this.ToSql(), this.DbParameters);
        }
        #endregion

        #region SqlBuilder
        /// <summary>
        /// 转换成Sql语句
        /// </summary>
        /// <returns></returns>
        public override string ToSql()
        {
            this.parameterIndex = 0;
            return string.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, fieldString, valueString);
        }
        #endregion
    }
}