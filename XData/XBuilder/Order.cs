using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
using XData.Core.ExpressionVisitors;

namespace XData.XBuilder
{
    /// <summary>
    /// 排序命令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class Order<T> : SqlBuilber
    {
        #region Fields
        private readonly SqlBuilber privoder;
        private readonly Dictionary<Expression, bool> sorts = new Dictionary<Expression, bool>();
        #endregion

        #region Constuctors
        internal Order(XContext context, SqlBuilber privoder) : base(context)
        {
            this.privoder = privoder;
        }
        private Order(XContext context, SqlBuilber privoder, Dictionary<Expression, bool> sorts) : this(context, privoder)
        {
            this.sorts = sorts.ToDictionary(x => x.Key, x => x.Value);
        }
        #endregion

        /// <summary>
        /// 根据一个字段排序
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public void By<TProperty>(Expression<Func<T, TProperty>> expression, bool isAsc = true)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }

            sorts.Add(expression.Body, isAsc);
        }
        /// <summary>
        /// 转换成Sql语句
        /// </summary>
        /// <returns></returns>
        public override string ToSql()
        {
            var orders = new Strings();
            var columns = new List<string>();
            foreach (var pair in sorts)
            {
                var exp = pair.Key;
                var sql = SqlExpressionVistor.Visit(exp, privoder);
                if (!columns.Contains(exp.ToString()))
                {
                    var sc = pair.Value ? "ASC" : "DESC";
                    columns.Add(exp.ToString());
                    orders.Add(string.Format("{0} {1}", sql, sc));
                }
                else
                {
                    throw Error.Exception("排序依据列表中的依据不能重复：" + sql);
                }
            }
            if (columns.Any())
            {
                return " ORDER BY " + orders;
            }
            return string.Empty;
        }

        internal Order<T> Copy(SqlBuilber newPrivoder)
        {
            return new Order<T>(this.Context, newPrivoder, this.sorts);
        }
    }
}