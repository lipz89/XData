using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
using XData.Core;
using XData.Core.ExpressionVisitors;
using XData.Extentions;
using XData.Meta;

namespace XData.XBuilder
{
    /// <summary>
    /// 查询命令
    /// 每一次改变Where，Order，Distinct，Top条件都会清除上一次缓存的查询结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Query<T> : SqlBuilber, IQuery<T>
    {
        #region Fields

        protected internal bool _useCache;
        protected Where<T> where;
        protected Order<T> order;
        protected bool _dist;
        protected int? _top;
        protected List<T> _list;
        protected string _fieldsPart;
        //protected Dictionary<string, string> fieldSqls = new Dictionary<string, string>();
        private LambdaExpression selector;

        //protected string _innerFieldsPart;

        #endregion

        #region Properties
        internal string DistinctPart
        {
            get
            {
                return _dist ? " DISTINCT " : string.Empty;
            }
        }
        internal string TopPart
        {
            get
            {
                var top = string.Empty;
                if (_top.HasValue && _top.Value >= 0)
                {
                    top = " TOP " + _top.Value + " ";
                }
                return top;
            }
        }
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
        internal string WherePart
        {
            get
            {
                var wherePart = string.Empty;
                if (this.where != null)
                {
                    this.where.parameterIndex = this.parameterIndex;
                    wherePart = this.where.ToSql();
                }
                return wherePart;
            }
        }
        internal string OrderPart
        {
            get
            {
                var orderPart = string.Empty;
                if (this.order != null)
                {
                    orderPart = this.order.ToSql();
                }
                return orderPart;
            }
        }

        #endregion

        #region Contructors

        /// <summary>
        /// 构造一个查询命令
        /// </summary>
        /// <param name="context"></param>
        internal Query(XContext context) : base(context)
        {
            this.tableMeta = TableMeta.From<T>();
            this.tableName = this.tableMeta.TableName;
        }
        /// <summary>
        /// 构造一个查询命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tableName"></param>
        /// <param name="useCache"></param>
        internal Query(XContext context, string tableName, bool useCache = true) : this(context)
        {
            if (tableMeta.IsSimpleType())
            {
                throw Error.Exception("查询的实体类型不正确。");
            }
            if (!tableName.IsNullOrWhiteSpace())
            {
                this.tableName = tableName;
            }
            this.namedType = new NamedType(this.tableMeta.Type, this.tableName);
            this.typeVisitor.Add(this.namedType);
            //typeNames.Add(typeof(T), _tableName);
            this._useCache = useCache;

            this.InitFieldsPart();
        }
        #endregion

        #region Where
        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IQuery<T> Where(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }
            this.where = this.where ?? new Where<T>(Context, this);
            this.where.Add(expression);
            this._list = null;
            return this;
        }
        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IQuery<T> WhereOr(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNullException(nameof(expression));
            }
            this.where = this.where ?? new Where<T>(Context, this);
            this.where.AddOr(expression);
            this._list = null;
            return this;
        }
        /// <summary>
        /// 清除查询条件
        /// </summary>
        /// <returns></returns>
        public IQuery<T> ClearWhere()
        {
            this.where = null;
            this._list = null;
            return this;
        }

        #endregion

        #region Order
        /// <summary>
        /// 排序条件
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public IQuery<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> expression, bool isAsc = true)
        {
            this.order = this.order ?? new Order<T>(Context, this);
            this.order.By(expression, isAsc);
            this._list = null;
            return this;
        }
        /// <summary>
        /// 多字段升序
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public IQuery<T> OrderBy(params Expression<Func<T, dynamic>>[] expressions)
        {
            this.order = this.order ?? new Order<T>(Context, this);
            this.order.ByAsc(expressions);
            this._list = null;
            return this;
        }
        /// <summary>
        /// 多字段降序
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public IQuery<T> OrderByDescending(params Expression<Func<T, dynamic>>[] expressions)
        {
            this.order = this.order ?? new Order<T>(Context, this);
            this.order.ByDesc(expressions);
            this._list = null;
            return this;
        }
        /// <summary>
        /// 清除排序条件
        /// </summary>
        /// <returns></returns>
        public IQuery<T> ClearOrder()
        {
            this.order = null;
            this._list = null;
            return this;
        }

        #endregion

        #region Top
        /// <summary>
        /// 设置查询的条数
        /// </summary>
        /// <param name="top">大于0表示查询条数，否则表示查询所有</param>
        /// <returns></returns>
        public IQuery<T> Top(int top)
        {
            if (top > 0)
            {
                _top = top;
            }
            else
            {
                _top = null;
            }
            this._list = null;
            return this;
        }

        #endregion

        #region Distinct
        /// <summary>
        /// 返回非重复记录
        /// </summary>
        /// <param name="distinct">true表示返回非重复数据，否则表示返回所有数据</param>
        /// <returns>查询构建器</returns>
        public IQuery<T> Distinct(bool distinct = true)
        {
            _dist = distinct;
            this._list = null;
            return this;
        }
        #endregion

        #region Select

        /// <summary>
        /// 投影到<typeparamref name="TResult"/>的查询
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> result)
        {
            if (result == null)
            {
                throw Error.ArgumentNullException(nameof(result));
            }
            if (typeof(T) == typeof(TResult))
            {
                this.selector = result;
                this.SetInnerFields();
                return (IQuery<TResult>)this;
            }
            var _tableName = this.tableName + "_";
            //this.innerTableMeta = TableMeta.From<TResult>(tableName);
            return new ComplexQuery<T, TResult>(this, _tableName, result/*, outFieldsSql*/);
        }


        #endregion

        #region Aggregate
        /// <summary>
        /// 计算查询结果的记录数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            if (this._useCache && this._list != null)
            {
                return this._list.Count;
            }
            var tableName = GetTableNameOrInnerSql();
            var sql = string.Format("SELECT COUNT(1) FROM {0} {1}", tableName, this.WherePart);
            return (int)Context.ExecuteScalar(sql, CommandType.Text, this.DbParameters);
        }
        /// <summary>
        /// 聚合函数
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="selector"></param>
        /// <param name="aggregateName"></param>
        /// <returns></returns>
        private TAggregate Aggregate<TAggregate>(Expression<Func<T, TAggregate>> selector, string aggregateName)
        {
            if (this._useCache && this._list != null)
            {
                dynamic _dynamiclist = this._list;
                var func = selector.Compile();
                switch (aggregateName)
                {
                    case "MAX":
                        return this._list.Max(func);
                    case "MIN":
                        return this._list.Min(func);
                    case "SUM":
                        return (TAggregate)_dynamiclist.Sum(func);
                    case "AVG":
                        return (TAggregate)_dynamiclist.Average(func);
                }
            }
            var tableName = GetTableNameOrInnerSql();
            var columnName = SqlExpressionVistor.Visit(selector, this);
            var sql = string.Format("SELECT {5}({4}{3}) FROM {0} {1} {2}", tableName, this.WherePart, this.OrderPart, columnName, this.DistinctPart, aggregateName);
            return (TAggregate)Context.ExecuteScalar(sql, CommandType.Text, this.DbParameters);
        }
        /// <summary>
        /// 查询结果中的最大值
        /// </summary>
        /// <typeparam name="TMax"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public TMax Max<TMax>(Expression<Func<T, TMax>> selector)
        {
            return this.Aggregate(selector, "MAX");
        }
        /// <summary>
        /// 查询结果中的最小值
        /// </summary>
        /// <typeparam name="TMin"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public TMin Min<TMin>(Expression<Func<T, TMin>> selector)
        {
            return this.Aggregate(selector, "MIN");
        }

        /// <summary>
        /// 查询结果中所有值的和
        /// </summary>
        /// <typeparam name="TSum"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public TSum Sum<TSum>(Expression<Func<T, TSum>> selector)
        {
            return this.Aggregate(selector, "SUM");
        }
        /// <summary>
        /// 查询结果中的平均值
        /// </summary>
        /// <typeparam name="TAvg"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public TAvg Avg<TAvg>(Expression<Func<T, TAvg>> selector)
        {
            return this.Aggregate(selector, "AVG");
        }

        #endregion

        #region Result
        /// <summary>
        /// 将查询结果输出到列表中
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            if (this._useCache)
            {
                if (_list == null)
                {
                    var enumer = Context.SqlQuery<T>(this.ToSql(), this.DbParameters);
                    _list = enumer.ToList();
                }
                return _list.ToList();
            }
            else
            {
                var enumer = Context.SqlQuery<T>(this.ToSql(), this.DbParameters);
                return enumer.ToList();
            }
        }
        #endregion

        #region Page
        /// <summary>
        /// 提取查询的分页数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Page<T> ToPage(int pageIndex, int pageSize)
        {
            return this.ToPage(new Page { PageSize = pageSize, PageIndex = pageIndex });
        }
        /// <summary>
        /// 提取查询的分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public Page<T> ToPage(Page page)
        {
            if (page == null)
            {
                throw Error.ArgumentNullException(nameof(page));
            }
            if (this._useCache && this._list != null)
            {
                var datas = this._list.Skip(page.Skip).Take(page.PageSize).ToList();
                var total = this._list.Count;
                return new Page<T>
                {
                    PageSize = page.PageSize,
                    PageIndex = page.PageIndex,
                    TotalRecords = total,
                    Items = datas
                };
            }
            else
            {
                var datas = Context.SqlQuery<T>(page, this.ToSql(), CommandType.Text, this.DbParameters).ToList();
                var total = this.Count();
                return new Page<T>
                {
                    PageSize = page.PageSize,
                    PageIndex = page.PageIndex,
                    TotalRecords = total,
                    Items = datas
                };
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
            this.parameterIndex = 0;
            var _tableName = GetTableNameOrInnerSql();
            var fieldsPart = this.InitFieldsPart();
            return string.Format("SELECT {3} {4} {5} FROM {0} {1} {2}", _tableName, this.WherePart, this.OrderPart, this.DistinctPart, this.TopPart, fieldsPart);
        }

        /// <summary>
        /// 转换成Sql子查询语句
        /// </summary>
        /// <returns></returns>
        protected internal virtual string ToInnerSql()
        {
            this.parameterIndex = 0;
            var _tableName = GetTableNameOrInnerSql();
            var fieldsPart = this.InitFieldsPart();
            return string.Format("SELECT {2} {3} {4} FROM {0} {1}", _tableName, this.WherePart, this.DistinctPart, this.TopPart, fieldsPart);
        }

        #endregion

        #region ProtectedMethods

        /// <summary>
        /// 获取子查询Sql或表名
        /// </summary>
        /// <returns></returns>
        protected virtual string GetTableNameOrInnerSql()
        {
            return EscapeSqlIdentifier(tableName);
        }

        /// <summary>
        /// 获取查询字段部分
        /// </summary>
        /// <returns></returns>
        protected virtual object InitFieldsPart()
        {
            //return "*";
            if (_fieldsPart.IsNullOrWhiteSpace())
            {
                var strs = new Strings();
                foreach (var column in tableMeta.Columns)
                {
                    var sql = EscapeSqlIdentifier(column.ColumnName);
                    strs.Add(sql);
                    namedType.Add(column.Member, sql);
                }
                _fieldsPart = strs.ToString();
            }
            return _fieldsPart;
        }

        /// <summary>
        /// 获取子查询字段部分
        /// </summary>
        /// <returns></returns>
        protected virtual void SetInnerFields()
        {
            var strs = new Strings();
            //var outerstrs = new Strings();

            var newExp = selector.Body as NewExpression;

            if (selector.Body is MemberInitExpression)
            {
                var exp = (MemberInitExpression)selector.Body;
                newExp = exp.NewExpression;
                foreach (var binding in exp.Bindings)
                {
                    var me = binding as MemberAssignment;
                    var sql = SqlExpressionVistor.Visit(me.Expression, this);
                    var field = EscapeSqlIdentifier(binding.Member.Name);
                    //var osql = EscapeSqlIdentifier(outTableName) + "." + field;
                    strs.Add(string.Format("{0} AS {1}", sql, field));
                    namedType.Add(binding.Member, sql);
                    //outerstrs.Add(osql);
                }
            }

            var inits = newExp.Arguments;
            var mems = newExp.Members;
            for (int i = 0; i < mems?.Count; i++)
            {
                if (inits[i] is MemberExpression)
                {
                    var sql = SqlExpressionVistor.Visit(inits[i], this);
                    var field = EscapeSqlIdentifier(mems[i].Name);
                    //var osql = EscapeSqlIdentifier(outTableName) + "." + field;
                    strs.Add(string.Format("{0} AS {1}", sql, field));
                    namedType.Add(mems[i], sql);
                    //outerstrs.Add(osql);
                }
                //else if (inits[i] is NewExpression) { }
                else
                {
                    throw Error.NotSupportedException("不支持复杂的类型初始化。");
                }
            }
            _fieldsPart = strs.ToString();
        }

        #endregion

        //internal override string GetFieldSql(string field)
        //{
        //    return fieldSqls[field];
        //}
    }
}