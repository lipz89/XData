using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
using XData.Core;
using XData.Core.ExpressionVisitors;
using XData.Extentions;

namespace XData.XBuilder
{
    /// <summary>
    /// 复合查询
    /// </summary>
    /// <typeparam name="TInner"></typeparam>
    /// <typeparam name="T"></typeparam>
    internal class ComplexQuery<TInner, T> : Query<T>
    {
        #region Fields
        private readonly Query<TInner> innerQuery;
        private readonly Expression<Func<TInner, T>> selector;
        #endregion

        #region Properties

        /// <summary>
        /// 内部数据提供者
        /// </summary>
        public IQuery<TInner> InnerQuery
        {
            get
            {
                return innerQuery;
            }
        }

        public override IReadOnlyList<object> Parameters
        {
            get
            {
                var ps = this._parameters.ToList();
                if (this.innerQuery != null)
                {
                    ps.AddRange(this.innerQuery.Parameters);
                }
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
        /// 构造一个复合查询
        /// </summary>
        /// <param name="innerQuery"></param>
        /// <param name="tableName"></param>
        /// <param name="result"></param>
        internal ComplexQuery(Query<TInner> innerQuery, string tableName, Expression<Func<TInner, T>> result)
            : base(innerQuery.Context)
        {
            this._useCache = innerQuery._useCache;
            this.innerQuery = innerQuery;
            this.selector = result;
            if (!tableName.IsNullOrWhiteSpace())
            {
                this.tableName = tableName;
            }
            this.namedType = new NamedType(this.tableMeta.Type, this.tableName);
            this.typeVisitor.Add(this.namedType);
            this.typeVisitor.Add(typeof(TInner), this.tableName);
            this.InitFieldsPart();
        }

        #endregion

        #region Override Query<T> Methods

        protected override string GetTableNameOrInnerSql()
        {
            var innerSql = this.innerQuery.ToInnerSql();
            return string.Format("({0}) AS {1}", innerSql, EscapeSqlIdentifier(tableName));
        }

        protected override object InitFieldsPart()
        {
            if (_fieldsPart.IsNullOrWhiteSpace())
            {
                if (tableMeta.IsSimpleType())
                {
                    var sql = SqlExpressionVistor.Visit(this.selector, this);
                    _fieldsPart = string.Format("{0} AS Field", sql);
                    namedType.AddDefault(sql);
                }
                else
                {
                    var strs = new Strings();
                    GetMembers(selector.Body, strs, null);
                    _fieldsPart = strs.ToString();
                }
            }
            return _fieldsPart;
        }

        #endregion
    }
}