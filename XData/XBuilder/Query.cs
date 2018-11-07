using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XData.Common;
using XData.Core;
using XData.Core.ExpressionVisitors;
using XData.Extentions;
using XData.Meta;
using XData.XBuilder.Include;

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

        protected Where<T> where;
        protected Order<T> order;
        protected bool isDistinct;
        protected int? topCount;
        protected string fieldsPart;
        protected LambdaExpression resultSelector;
        private string wherePart;
        private string orderPart;
        protected List<IInclude<T>> includes;

        #endregion

        #region Properties

        private string DistinctPart
        {
            get
            {
                return isDistinct ? " DISTINCT " : string.Empty;
            }
        }

        private string TopPart
        {
            get
            {
                var topPart = string.Empty;
                if (this.topCount.HasValue && this.topCount.Value >= 0)
                {
                    topPart = " TOP " + this.topCount.Value + " ";
                }
                return topPart;
            }
        }
        /// <summary>
        /// 参数列表
        /// </summary>
        public override IReadOnlyList<object> Parameters
        {
            get
            {
                var ps = this.parameters.ToList();
                if (this.where != null)
                {
                    ps.AddRange(this.where.parameters);
                }
                return ps.AsReadOnly();
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
        ///// <param name="useCache"></param>
        internal Query(XContext context, string tableName/*, bool useCache = true*/) : this(context)
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

            //this.InitFieldsPart();
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

            var query = Copy();
            query.where = query.where ?? new Where<T>(Context, query);
            query.where.Add(expression);
            query.wherePart = null;
            return query;
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
        private IQuery<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> expression, bool isAsc)
        {
            var query = Copy();
            query.order = query.order ?? new Order<T>(Context, query);
            query.order.By(expression, isAsc);
            return query;
        }
        /// <summary>
        /// 多字段升序
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IQuery<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return this.OrderBy(expression, true);
        }
        /// <summary>
        /// 多字段降序
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IQuery<T> OrderByDescending<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return this.OrderBy(expression, false);
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
            if (top == this.topCount)
            {
                return this;
            }
            var query = Copy();
            if (top > 0)
            {
                query.topCount = top;
            }
            else
            {
                query.topCount = null;
            }
            return query;
        }

        #endregion

        #region Distinct
        /// <summary>
        /// 返回非重复记录
        /// </summary>
        /// <returns>查询构建器</returns>
        public IQuery<T> Distinct()
        {
            if (this.isDistinct)
            {
                return this;
            }
            var query = Copy();
            query.isDistinct = true;
            return this;
        }
        #endregion

        #region Include

        private Expression<Func<T1, TKey>> GetKeySelector<T1, TKey>()
        {
            var key = MapperConfig.GetKeyMeta<T1>();
            if (key != null && key.Member != null && typeof(TKey).IsAssignableFrom(key.Member.GetMemberType()))
            {
                return key.Member.GetMemberProperty<T1, TKey>();
            }

            return null;
        }

        private List<T> FillInclude(List<T> result)
        {
            if (!includes.IsNullOrEmpty())
            {
                foreach (var include in includes)
                {
                    result = include.Invoke(result);
                }
            }

            return result;
        }
        private T FillInclude(T item)
        {
            if (!includes.IsNullOrEmpty())
            {
                foreach (var include in includes)
                {
                    item = include.Invoke(item);
                }
            }

            return item;
        }

        public IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, TRelaction>> property, Expression<Func<T, TKey>> tkey, Expression<Func<TRelaction, TKey>> relectionKey, Action<T, TRelaction> action)
        {
            if (property == null) { throw Error.ArgumentNullException(nameof(property)); }
            if (tkey == null) { throw Error.ArgumentNullException(nameof(tkey)); }
            if (relectionKey == null) { throw Error.ArgumentNullException(nameof(relectionKey)); }
            if (action == null) { throw Error.ArgumentNullException(nameof(action)); }

            var query = Copy();
            query.includes = query.includes ?? new List<IInclude<T>>();
            query.includes.Add(new IncludePropertyInfo<T, TRelaction, TKey>(Context, tkey, relectionKey, action));
            return query;
        }

        public IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, TRelaction>> property, Expression<Func<T, TKey>> tkey, Action<T, TRelaction> action)
        {
            return this.Include<TRelaction, TKey>(property, tkey, GetKeySelector<TRelaction, TKey>(), action);
        }

        public IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, TRelaction>> property, Expression<Func<TRelaction, TKey>> relectionKey, Action<T, TRelaction> action)
        {
            return this.Include<TRelaction, TKey>(property, GetKeySelector<T, TKey>(), relectionKey, action);
        }

        public IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, ICollection<TRelaction>>> property, Expression<Func<T, TKey>> tkey, Expression<Func<TRelaction, TKey>> relectionKey, Action<T, IEnumerable<TRelaction>> action)
        {
            if (property == null) { throw Error.ArgumentNullException(nameof(property)); }
            if (tkey == null) { throw Error.ArgumentNullException(nameof(tkey)); }
            if (relectionKey == null) { throw Error.ArgumentNullException(nameof(relectionKey)); }
            if (action == null) { throw Error.ArgumentNullException(nameof(action)); }

            var query = Copy();
            query.includes = query.includes ?? new List<IInclude<T>>();
            query.includes.Add(new IncludeCollectionInfo<T, TRelaction, TKey>(Context, tkey, relectionKey, action));
            return query;
        }

        public IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, ICollection<TRelaction>>> property, Expression<Func<T, TKey>> tkey, Action<T, IEnumerable<TRelaction>> action)
        {
            return this.Include<TRelaction, TKey>(property, tkey, GetKeySelector<TRelaction, TKey>(), action);
        }

        public IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, ICollection<TRelaction>>> property, Expression<Func<TRelaction, TKey>> relectionKey, Action<T, IEnumerable<TRelaction>> action)
        {
            return this.Include<TRelaction, TKey>(property, GetKeySelector<T, TKey>(), relectionKey, action);
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
                var query = Copy();
                query.resultSelector = result;
                query.SetInnerFields();
                return (IQuery<TResult>)query;
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
            this.parameterIndex = 0;
            var _tableName = GetTableNameOrInnerSql();
            var sql = string.Format("SELECT COUNT(1) FROM {0} {1}", _tableName, this.GetWherePart());
            return (int)Context.ExecuteScalar(sql, this.DbParameters);
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
            this.parameterIndex = 0;
            var _tableName = GetTableNameOrInnerSql();
            var columnName = SqlExpressionVistor.Visit(selector, this);
            var sql = string.Format("SELECT {5}({4}{3}) FROM {0} {1} {2}", _tableName, this.GetWherePart(), this.GetOrderPart(), columnName, this.DistinctPart, aggregateName);
            var value = Context.ExecuteScalar(sql, this.DbParameters);
            if (value == DBNull.Value)
            {
                if (typeof(TAggregate).IsValueType && (aggregateName == "SUM" || aggregateName == "AVG"))
                {
                    return default(TAggregate);
                }

                throw new XDataException("聚合结果为DBNull。");
            }
            return (TAggregate)value;
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
            var enumer = Context.SqlQuery<T>(this.ToSql(), this.DbParameters);
            var list = enumer.ToList();
            return FillInclude(list);
        }

        public T FirstOrDefault()
        {
            var enumer = Context.SqlQuery<T>(this.ToSql(), this.DbParameters);
            return FillInclude(enumer.FirstOrDefault());
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

            var datas = Context.SqlQuery<T>(page, this.ToSql(), CommandType.Text, this.DbParameters).ToList();
            var total = this.Count();
            return new Page<T>
            {
                PageSize = page.PageSize,
                PageIndex = page.PageIndex,
                TotalRecords = total,
                Items = FillInclude(datas)
            };
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
            var _fieldsPart = this.InitFieldsPart();
            return string.Format("SELECT {3} {4} {5} FROM {0} {1} {2}", _tableName, this.GetWherePart(), this.GetOrderPart(), this.DistinctPart, this.TopPart, _fieldsPart);
        }

        /// <summary>
        /// 转换成Sql子查询语句
        /// </summary>
        /// <returns></returns>
        protected internal virtual string ToInnerSql()
        {
            this.parameterIndex = 0;
            var _tableName = GetTableNameOrInnerSql();
            var _fieldsPart = this.InitFieldsPart();
            return string.Format("SELECT {2} {3} {4} FROM {0} {1}", _tableName, this.GetWherePart(), this.DistinctPart, this.TopPart, _fieldsPart);
        }

        #endregion

        #region ProtectedMethods

        private string GetWherePart()
        {
            if (this.wherePart.IsNullOrWhiteSpace() && this.where != null)
            {
                this.wherePart = this.where.ToSql();
            }
            return this.wherePart;
        }

        private string GetOrderPart()
        {
            if (this.orderPart.IsNullOrWhiteSpace() && this.order != null)
            {
                this.orderPart = this.order.ToSql();
            }
            return this.orderPart;
        }

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
            if (fieldsPart.IsNullOrWhiteSpace())
            {
                var strs = new Strings();
                foreach (var column in tableMeta.Columns)
                {
                    var sql = EscapeSqlIdentifier(column.ColumnName);
                    strs.Add(sql);
                    namedType.Add(column.Member, sql);
                }
                fieldsPart = strs.ToString();
            }
            return fieldsPart;
        }

        /// <summary>
        /// 获取子查询字段部分
        /// </summary>
        /// <returns></returns>
        protected virtual void SetInnerFields()
        {
            var strs = new Strings();
            GetMembers(resultSelector.Body, strs, null);
            fieldsPart = strs.ToString();
        }

        protected void GetMembers(Expression expression, Strings strs, MemberInfo member, string path = null)
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
                strs.Add(string.Format("{0} AS {1}", sql, field));
                namedType.Add(member, sql);
                return;
            }

            var newExp = expression as NewExpression;
            if (expression is MemberInitExpression)
            {
                var exp = (MemberInitExpression)expression;
                newExp = exp.NewExpression;
                foreach (var binding in exp.Bindings)
                {
                    var me = binding as MemberAssignment;
                    GetMembers(me.Expression, strs, binding.Member, path);
                }
            }

            var inits = newExp.Arguments;
            var mems = newExp.Members;
            for (int i = 0; i < mems?.Count; i++)
            {
                if (inits[i] is MemberExpression)
                {
                    GetMembers(inits[i], strs, mems[i], path);
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

        #endregion

        protected virtual Query<T> Copy()
        {
            var query = new Query<T>(this.Context, this.tableName);
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