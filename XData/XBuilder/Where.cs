using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using XData.Core.ExpressionVisitors;

namespace XData.XBuilder
{
    /// <summary>
    /// 查询条件构造器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class Where<T> : SqlBuilber
    {
        #region Fields
        private Expression _expression;
        private Query<T> _privoder;
        #endregion

        #region Constuctors
        internal Where(XContext context, Query<T> privoder = null) : base(context)
        {
            this._privoder = privoder;
        }
        #endregion

        #region Methods
        /// <summary>
        /// 添加一个查询条件
        /// </summary>
        /// <param name="expression"></param>
        public void Add(Expression<Func<T, bool>> expression)
        {
            if (_expression == null)
            {
                _expression = expression.Body;
                _parameter = expression.Parameters[0];
            }
            else
            {
                _expression = Expression.AndAlso(_expression, ParameterReplaceVisitor.Replace(expression.Body, _parameter));
            }
        }
        /// <summary>
        /// 添加一个查询条件
        /// </summary>
        /// <param name="expression"></param>
        public void AddOr(Expression<Func<T, bool>> expression)
        {
            if (_expression == null)
            {
                _expression = expression.Body;
                _parameter = expression.Parameters[0];
            }
            else
            {
                _expression = Expression.OrElse(_expression, ParameterReplaceVisitor.Replace(expression.Body, _parameter));
            }
        }
        #endregion

        #region SqlBuilder

        /// <summary>
        /// 转换成Sql语句
        /// </summary>
        /// <returns></returns>
        public override string ToSql()
        {
            var typeNames = this._privoder?._typeNames ?? new Dictionary<Type, string> { { typeof(T), GetTableName<T>() } };
            var wb = new WhereExpressionVistor(_expression, this, typeNames).ToSql();
            return string.Format(" WHERE ({0})", wb);
        }

        #endregion
    }
}