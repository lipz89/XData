using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
using XData.Extentions;

namespace XData.XBuilder
{
    /// <summary>
    /// 排序命令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class Order<T> : SqlBuilber
    {
        #region Fields
        private readonly List<string> sorts = new List<string>();
        private readonly Strings orders = new Strings();
        #endregion

        #region Constuctors
        internal Order(XContext context) : base(context)
        {
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

            var member = expression.GetMember();
            if (member != null)
            {
                var columnName = GetTableName<T>() + "." + GetColumnName(expression);
                if (!sorts.Contains(columnName))
                {
                    sorts.Add(columnName);
                    orders.Add(string.Format("{0} {1}", columnName, isAsc ? "ASC" : "DESC"));
                }
                else
                {
                    throw Error.ArgumentException("排序字段已经存在。", nameof(expression));
                }
            }
        }

        /// <summary>
        /// 根据多字段升序
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public void ByAsc(params Expression<Func<T, dynamic>>[] expressions)
        {
            foreach (var expression in expressions)
            {
                By(expression, true);
            }
        }

        /// <summary>
        /// 根据多字段降序
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public void ByDesc(params Expression<Func<T, dynamic>>[] expressions)
        {
            foreach (var expression in expressions)
            {
                By(expression, false);
            }
        }
        /// <summary>
        /// 转换成Sql语句
        /// </summary>
        /// <returns></returns>
        public override string ToSql()
        {
            if (sorts.Any())
            {
                return " ORDER BY " + orders;
            }
            return string.Empty;
        }
    }
}