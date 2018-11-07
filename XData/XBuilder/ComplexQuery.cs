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
    internal sealed class ComplexQuery<TInner, T> : Query<T>
    {
        #region Fields
        private readonly Query<TInner> innerQuery;
        #endregion

        #region Properties

        public override IReadOnlyList<object> Parameters
        {
            get
            {
                var ps = this.parameters.ToList();
                if (this.innerQuery != null)
                {
                    ps.AddRange(this.innerQuery.Parameters);
                }
                if (this.where != null)
                {
                    ps.AddRange(this.where.parameters);
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
        internal ComplexQuery(Query<TInner> innerQuery, string tableName, LambdaExpression result)
            : base(innerQuery.Context)
        {
            this.innerQuery = innerQuery;
            if (!tableName.IsNullOrWhiteSpace())
            {
                this.tableName = tableName;
            }

            this.resultSelector = result;
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
            if (fieldsPart.IsNullOrWhiteSpace())
            {
                if (tableMeta.IsSimpleType())
                {
                    var sql = SqlExpressionVistor.Visit(this.resultSelector, this);
                    fieldsPart = string.Format("{0} AS Field", sql);
                    namedType.AddDefault(sql);
                }
                else
                {
                    var strs = new Strings();
                    GetMembers(resultSelector.Body, strs, null);
                    fieldsPart = strs.ToString();
                }
            }
            return fieldsPart;
        }

        #endregion

        protected override Query<T> Copy()
        {
            var query = new ComplexQuery<TInner, T>(this.innerQuery, this.tableName, this.resultSelector);
            query.order = this.order?.Copy(query);
            query.where = this.where?.Copy(query);
            query.fieldsPart = this.fieldsPart;
            query.includes = this.includes?.ToList();
            query.isDistinct = this.isDistinct;
            query.resultSelector = this.resultSelector;
            query.topCount = this.topCount;
            return query;
        }
    }
}