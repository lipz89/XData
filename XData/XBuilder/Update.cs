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
    /// 更新命令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Update<T> : SqlBuilber, IExecutable
    {
        #region Fields
        private bool _hasWhere;
        private readonly Strings setterString = new Strings();
        private readonly ColumnMeta keyMeta;
        private Where<T> where;
        #endregion

        #region Properties
        /// <summary>
        /// 参数列表
        /// </summary>
        public override IReadOnlyList<object> Parameters
        {
            get
            {
                var ps = this._parameters.ToList();
                if (this.where != null)
                {
                    ps.AddRange(this.where._parameters);
                }
                return ps.AsReadOnly();
            }
        }
        #endregion

        #region Constuctors

        private Update(XContext context)
            : base(context)
        {
            this.tableMeta = TableMeta.From<T>();
            this.keyMeta = tableMeta.Key;
            this.tableName = this.tableMeta.TableName;
            this.namedType = new NamedType(this.tableMeta.Type, this.tableName);
            this.typeVisitor.Add(this.namedType);
        }

        /// <summary>
        /// 根据两个实体的比较构造一个更新命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        /// <param name="primaryKey"></param>
        internal Update(XContext context, T oldEntity, T newEntity, Expression<Func<T, object>> primaryKey = null)
            : this(context)
        {
            if (oldEntity == null)
            {
                throw Error.ArgumentNullException(nameof(oldEntity));
            }
            if (newEntity == null)
            {
                throw Error.ArgumentNullException(nameof(newEntity));
            }

            var columns = tableMeta.Columns.Where(x => x.Member != keyMeta?.Member).ToList();
            foreach (var column in columns)
            {
                var memAccess = column.Member.GetMemberAccess<T>()?.Compile();
                if (memAccess != null)
                {
                    var oldValue = memAccess(oldEntity);
                    var newValue = memAccess(newEntity);
                    if (!Equals(oldValue, newValue))
                    {
                        this.setterString.Add(string.Format("{0}={1}",
                            EscapeSqlIdentifier(column.ColumnName), GetParameterIndex()));
                        this._parameters.Add(newValue);
                    }
                }
            }
            if (!this._parameters.Any())
            {
                throw Error.Exception("必须更新至少一个字段。");
            }

            AddConditionByKey(oldEntity, primaryKey);
        }

        private void AddConditionByKey(T oldEntity, Expression<Func<T, object>> primaryKey)
        {
            var exp = keyMeta?.Expression as LambdaExpression ?? primaryKey;
            if (exp != null)
            {
                var val = exp.Compile().DynamicInvoke(oldEntity);
                var mem = exp.Body;
                if (mem is UnaryExpression)
                {
                    mem = (mem as UnaryExpression).Operand;
                }
                var newExp = Expression.Equal(mem, Expression.Constant(val));
                var lambda = Expression.Lambda<Func<T, bool>>(newExp, exp.Parameters);
                this.where = new Where<T>(Context, this);
                this.where.Add(lambda);
                _hasWhere = true;
            }
        }

        /// <summary>
        /// 根据指定实体构造一个更新指定字段的命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="newEntity"></param>
        /// <param name="include">包含或者排除，true包含表示仅更新指定的字段，false排除表示不更新指定的字段</param>
        /// <param name="fields"></param>
        internal Update(XContext context, T newEntity, bool include = false, params Expression<Func<T, object>>[] fields)
            : this(context)
        {
            if (newEntity == null)
            {
                throw Error.ArgumentNullException(nameof(newEntity));
            }

            var exceptFields = fields.Select(x => x.GetPropertyName());
            var columns = tableMeta.Columns.Where(x => x.Member != keyMeta?.Member);
            columns = columns.Where(x => exceptFields.Contains(x.Member.Name) == include).ToList();
            foreach (var column in columns)
            {
                var memAccess = column.Member.GetMemberAccess<T>()?.Compile();
                if (memAccess != null)
                {
                    var newValue = memAccess(newEntity);

                    this.setterString.Add(string.Format("{0}={1}",
                        EscapeSqlIdentifier(column.ColumnName), GetParameterIndex()));
                    this._parameters.Add(newValue);
                }
            }
            if (!this._parameters.Any())
            {
                throw Error.Exception("必须更新至少一个字段。");
            }

            AddConditionByKey(newEntity, null);
        }

        /// <summary>
        /// 根据指定的字段值列字典构造一个更新命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fieldValues"></param>
        internal Update(XContext context, IDictionary<string, object> fieldValues)
            : this(context)
        {
            if (fieldValues.IsNullOrEmpty())
            {
                throw Error.ArgumentNullException(nameof(fieldValues));
            }

            var columns = tableMeta.Columns.Where(x => x.Member != keyMeta?.Member && fieldValues.Keys.Contains(x.Member.Name)).ToList();
            foreach (var column in columns)
            {
                var memAccess = column.Member.GetMemberAccess<T>()?.Compile();
                if (memAccess != null)
                {
                    this.setterString.Add(string.Format("{0}={1}",
                        EscapeSqlIdentifier(column.ColumnName), GetParameterIndex()));
                    this._parameters.Add(fieldValues[column.Member.Name]);
                }
            }
            if (!this._parameters.Any())
            {
                throw Error.Exception("必须更新至少一个字段。");
            }
        }

        #endregion

        #region Where
        /// <summary>
        /// 更新条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Update<T> Where(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }
            if (_hasWhere)
            {
                throw Error.Exception("已经指定了主键列为Where条件。");
            }
            this.where = this.where ?? new Where<T>(Context, this);
            this.where.Add(expression);
            return this;
        }
        /// <summary>
        /// 更新条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Update<T> WhereOr(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }
            if (_hasWhere)
            {
                throw Error.Exception("已经指定了主键列为Where条件。");
            }
            this.where = this.where ?? new Where<T>(Context, this);
            this.where.AddOr(expression);
            return this;
        }
        /// <summary>
        /// 清除查询条件
        /// </summary>
        /// <returns></returns>
        public Update<T> ClearWhere()
        {
            if (!this._hasWhere)
            {
                this.where = null;
            }
            return this;
        }

        #endregion

        #region IExecutable
        /// <summary>
        /// 执行更新命令
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
            var wherePart = string.Empty;
            if (this.where != null)
            {
                this.where.parameterIndex = this.parameterIndex;
                wherePart = this.where.ToSql();
            }
            return string.Format("UPDATE {0} SET {1} {2}", tableName, this.setterString, wherePart);
        }
        #endregion
    }
}