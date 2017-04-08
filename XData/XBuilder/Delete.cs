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
    /// 删除命令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Delete<T> : SqlBuilber, IExecutable
    {
        #region Fields
        private Where<T> where;
        private readonly bool _hasKeyWhere;
        private string _wherePart;
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
        /// <summary>
        /// 构造一个删除命令
        /// </summary>
        /// <param name="context"></param>
        internal Delete(XContext context) : base(context)
        {
            this.tableMeta = TableMeta.From<T>();
            this.tableName = this.tableMeta.TableName;
            this.namedType = new NamedType(this.tableMeta.Type, this.tableName);
            this.typeVisitor.Add(this.namedType);
        }

        internal Delete(XContext context, T entity, Expression<Func<T, object>> primaryKey = null)
            : this(context)
        {
            if (entity == null)
            {
                throw Error.ArgumentNullException(nameof(entity));
            }

            var exp = tableMeta.Key?.Expression as LambdaExpression;
            if (exp == null && primaryKey != null)
            {
                MapperConfig.HasKey(primaryKey);
                exp = primaryKey;
            }
            if (exp != null)
            {
                var val = exp.Compile().DynamicInvoke(entity);
                var keyExp = Expression.Constant(val);
                var mem = exp.Body.ChangeType(keyExp.Type);
                var newExp = Expression.Equal(mem, keyExp);
                var lambda = Expression.Lambda<Func<T, bool>>(newExp, exp.Parameters);
                this.where = new Where<T>(Context, this);
                this.where.Add(lambda);
                _hasKeyWhere = true;
            }
        }
        #endregion

        #region Where

        /// <summary>
        /// 设置删除条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Delete<T> Where(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }
            if (_hasKeyWhere)
            {
                throw Error.Exception("已经指定了主键列为Where条件。");
            }
            this.where = this.where ?? new Where<T>(Context, this);
            this.where.Add(expression);
            this._wherePart = null;
            return this;
        }

        /// <summary>
        /// 设置删除条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Delete<T> WhereOr(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }
            if (_hasKeyWhere)
            {
                throw Error.Exception("已经指定了主键列为Where条件。");
            }
            this.where = this.where ?? new Where<T>(Context, this);
            this.where.AddOr(expression);
            this._wherePart = null;
            return this;
        }
        /// <summary>
        /// 清除查询条件，只能清除手动添加的条件
        /// </summary>
        /// <returns></returns>
        public Delete<T> ClearWhere()
        {
            if (!this._hasKeyWhere)
            {
                this.where = null;
                this._wherePart = null;
            }
            return this;
        }

        #endregion

        #region IExecutable

        /// <summary>
        /// 执行删除操作
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            return Context.ExecuteNonQuery(this.ToSql(), this.DbParameters);
        }
        #endregion

        #region SqlBuilder
        /// <summary>
        /// 转换成Sql删除语句
        /// </summary>
        /// <returns></returns>
        public override string ToSql()
        {
            this.parameterIndex = 0;
            return string.Format("DELETE FROM {0} {1}", tableName, this.GetWherePart());
        }
        internal string GetWherePart()
        {
            if (this._wherePart.IsNullOrWhiteSpace() && this.where != null)
            {
                this._wherePart = this.where.ToSql();
            }
            return _wherePart;
        }
        #endregion
    }
}