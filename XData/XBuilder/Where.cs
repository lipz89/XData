using System;
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
        private Expression whereExpression;
        private ParameterExpression parameter;
        private readonly SqlBuilber privoder;
        #endregion

        #region Constuctors
        internal Where(XContext context, SqlBuilber privoder) : base(context)
        {
            this.privoder = privoder;
        }
        #endregion

        #region Methods
        /// <summary>
        /// 添加一个查询条件
        /// </summary>
        /// <param name="expression"></param>
        public void Add(Expression<Func<T, bool>> expression)
        {
            if (whereExpression == null)
            {
                whereExpression = expression.Body;
                parameter = expression.Parameters[0];
            }
            else
            {
                whereExpression = Expression.AndAlso(whereExpression, ParameterReplaceVisitor.Replace(expression.Body, parameter));
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
            var wb = SqlExpressionVistor.Visit(whereExpression, privoder);
            return string.Format(" WHERE {0}", wb);
        }

        #endregion

        internal Where<T> Copy(SqlBuilber newPrivoder)
        {
            return new Where<T>(Context, newPrivoder) { whereExpression = this.whereExpression, parameter = this.parameter };
        }
    }
}