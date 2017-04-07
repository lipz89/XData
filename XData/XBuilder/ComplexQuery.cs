using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        private Expression<Func<TInner, T>> selector;
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

        /// <summary>
        /// 内部数据提供者
        /// </summary>
        private Query<TInner> innerQuery;

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
                    //return EscapeSqlIdentifier(outTableName) + "." + EscapeSqlIdentifier("Field");
                }
                else
                {
                    var strs = new Strings();
                    //var outerstrs = new Strings();
                    GetMembers(selector.Body, strs, null);

                    //var newExp = selector.Body as NewExpression;

                    //if (selector.Body is MemberInitExpression)
                    //{
                    //    var exp = (MemberInitExpression)selector.Body;
                    //    newExp = exp.NewExpression;
                    //    foreach (var binding in exp.Bindings)
                    //    {
                    //        var me = binding as MemberAssignment;
                    //        var sql = SqlExpressionVistor.Visit(me.Expression, this);
                    //        var field = EscapeSqlIdentifier(binding.Member.Name);
                    //        //var osql = EscapeSqlIdentifier(outTableName) + "." + field;
                    //        strs.Add(string.Format("{0} AS {1}", sql, field));
                    //        namedType.Add(binding.Member, sql);
                    //        //outerstrs.Add(osql);
                    //    }
                    //}

                    //var inits = newExp.Arguments;
                    //var mems = newExp.Members;
                    //for (int i = 0; i < mems?.Count; i++)
                    //{
                    //    if (inits[i] is MemberExpression)
                    //    {
                    //        var sql = SqlExpressionVistor.Visit(inits[i], this);
                    //        var field = EscapeSqlIdentifier(mems[i].Name);
                    //        //var osql = EscapeSqlIdentifier(outTableName) + "." + field;
                    //        strs.Add(string.Format("{0} AS {1}", sql, field));
                    //        namedType.Add(mems[i], sql);
                    //        //outerstrs.Add(osql);
                    //    }
                    //    else if (inits[i] is NewExpression)
                    //    {

                    //    }
                    //    else
                    //    {
                    //        throw Error.NotSupportedException("不支持复杂的类型初始化。");
                    //    }
                    //}
                    _fieldsPart = strs.ToString();
                }
            }
            return _fieldsPart;
        }

        private void GetMembers(Expression expression, Strings strs, MemberInfo member, string path = null)
        {
            if (expression is MemberExpression)
            {
                var sql = SqlExpressionVistor.Visit(expression, this);
                var name = member.Name;
                if (!path.IsNullOrWhiteSpace())
                {
                    name = path + "-" + name;
                }
                var field = EscapeSqlIdentifier(name);
                //var osql = EscapeSqlIdentifier(outTableName) + "." + field;
                strs.Add(string.Format("{0} AS {1}", sql, field));
                namedType.Add(member, sql);
                return;
            }

            var newExp = expression as NewExpression;
            if (selector.Body is MemberInitExpression)
            {
                var exp = (MemberInitExpression)selector.Body;
                newExp = exp.NewExpression;
                foreach (var binding in exp.Bindings)
                {
                    var me = binding as MemberAssignment;
                    GetMembers(me.Expression, strs, binding.Member, path);
                    //var sql = SqlExpressionVistor.Visit(me.Expression, this);
                    //var field = EscapeSqlIdentifier(binding.Member.Name);
                    ////var osql = EscapeSqlIdentifier(outTableName) + "." + field;
                    //strs.Add(string.Format("{0} AS {1}", sql, field));
                    //namedType.Add(binding.Member, sql);
                    //outerstrs.Add(osql);
                }
            }

            var inits = newExp.Arguments;
            var mems = newExp.Members;
            for (int i = 0; i < mems?.Count; i++)
            {
                if (inits[i] is MemberExpression)
                {
                    GetMembers(inits[i], strs, mems[i], path);
                    //var sql = SqlExpressionVistor.Visit(inits[i], this);
                    //var field = EscapeSqlIdentifier(mems[i].Name);
                    ////var osql = EscapeSqlIdentifier(outTableName) + "." + field;
                    //strs.Add(string.Format("{0} AS {1}", sql, field));
                    //namedType.Add(mems[i], sql);
                    ////outerstrs.Add(osql);
                }
                else if (inits[i] is NewExpression)
                {
                    var _path = mems[i].Name;
                    if (!path.IsNullOrWhiteSpace())
                    {
                        _path = path + "-" + _path;
                    }
                    GetMembers(inits[i], strs, null, _path);
                }
                else
                {
                    throw Error.NotSupportedException("不支持复杂的类型初始化。");
                }
            }
        }
    }
}