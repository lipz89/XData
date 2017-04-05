using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
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
        private readonly bool _hasWhere;
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
        }

        internal Delete(XContext context, T entity, Expression<Func<T, object>> primaryKey = null) : base(context)
        {
            if (entity == null)
                throw Error.ArgumentNullException(nameof(entity));

            var keyMeta = MetaConfig.GetKeyMeta<T>();
            var exp = keyMeta?.Expression as LambdaExpression ?? primaryKey;
            if (exp != null)
            {
                var val = exp.Compile().DynamicInvoke(entity);
                var mem = exp.Body;
                if (mem is UnaryExpression)
                {
                    mem = (mem as UnaryExpression).Operand;
                }
                var newExp = Expression.Equal(mem, Expression.Constant(val));
                var lambda = Expression.Lambda<Func<T, bool>>(newExp, exp.Parameters);
                this.where = new Where<T>(Context);
                this.where.Add(lambda);
                _hasWhere = true;
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
            if (_hasWhere)
            {
                throw Error.Exception("已经指定了主键列为Where条件。");
            }
            this.where = this.where ?? new Where<T>(Context);
            this.where.Add(expression);
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
            if (_hasWhere)
            {
                throw Error.Exception("已经指定了主键列为Where条件。");
            }
            this.where = this.where ?? new Where<T>(Context);
            this.where.AddOr(expression);
            return this;
        }
        /// <summary>
        /// 清除查询条件，只能清除手动添加的条件
        /// </summary>
        /// <returns></returns>
        public Delete<T> ClearWhere()
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
            this._parameterIndex = 0;
            var tableName = GetTableName<T>();
            var wherePart = string.Empty;
            if (this.where != null)
            {
                this.where._parameterIndex = this._parameterIndex;
                wherePart = this.where.ToSql();
            }
            return string.Format("DELETE FROM {0} {1}", tableName, wherePart);
        }
        #endregion
    }
}