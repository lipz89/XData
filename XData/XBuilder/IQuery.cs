using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using XData.Common;

namespace XData.XBuilder
{
    /// <summary>
    /// 查询接口
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IQuery<T>
    {
        /// <summary>
        /// 上下文对象
        /// </summary>
        XContext Context { get; }

        /// <summary>
        /// 参数列表
        /// </summary>
        IReadOnlyList<object> Parameters { get; }
        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">条件表达式</param>
        /// <returns>当前查询</returns>
        IQuery<T> Where(Expression<Func<T, bool>> expression);

        /// <summary>
        /// 多字段升序
        /// </summary>
        /// <param name="expression">排序字段表达式数组</param>
        /// <returns>当前查询</returns>
        IQuery<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> expression);

        /// <summary>
        /// 多字段降序
        /// </summary>
        /// <param name="expression">排序字段表达式数组</param>
        /// <returns>当前查询</returns>
        IQuery<T> OrderByDescending<TProperty>(Expression<Func<T, TProperty>> expression);

        /// <summary>
        /// 设置查询的条数
        /// </summary>
        /// <param name="top">大于0表示查询条数，否则表示查询所有</param>
        /// <returns>当前查询</returns>
        IQuery<T> Top(int top);

        /// <summary>
        /// 返回非重复记录
        /// </summary>
        /// <returns>当前查询</returns>
        IQuery<T> Distinct();

        /// <summary>
        /// 投影
        /// </summary>
        /// <typeparam name="TResult">投影结果类型</typeparam>
        /// <param name="result">投影函数表达式</param>
        /// <returns>返回一个表示对投影结果的查询</returns>
        IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> result);

        /// <summary>
        /// 计算查询结果的记录数
        /// </summary>
        /// <returns>计数结果</returns>
        int Count();

        /// <summary>
        /// 查询结果中的最大值
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="selector">表达式</param>
        /// <returns>最大值</returns>
        TResult Max<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 查询结果中的最小值
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="selector">表达式</param>
        /// <returns>最小值</returns>
        TResult Min<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 查询结果中所有值的和
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="selector">表达式</param>
        /// <returns>所有值的和</returns>
        TResult Sum<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 查询结果中的平均值
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="selector">表达式</param>
        /// <returns>返回平均值</returns>
        TResult Avg<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 将查询结果输出到列表中
        /// </summary>
        /// <returns>返回当前查询的结果</returns>
        List<T> ToList();

        /// <summary>
        /// 将查询结果中的第一条记录返回
        /// </summary>
        /// <returns>返回当前查询的第一条结果</returns>
        T FirstOrDefault();

        /// <summary>
        /// 提取查询的分页数据
        /// </summary>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页宽</param>
        /// <returns>返回分页数据结果</returns>
        Page<T> ToPage(int pageIndex, int pageSize);

        /// <summary>
        /// 提取查询的分页数据
        /// </summary>
        /// <param name="page">分页参数</param>
        /// <returns>返回分页数据结果</returns>
        Page<T> ToPage(Page page);
        /// <summary>
        /// 转换成Sql语句
        /// </summary>
        /// <returns>返回sql语句字符串</returns>
        string ToSql();

        IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, TRelaction>> property, Expression<Func<T, TKey>> tkey, Expression<Func<TRelaction, TKey>> relectionKey, Action<T, TRelaction> action);

        IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, TRelaction>> property, Expression<Func<T, TKey>> tkey, Action<T, TRelaction> action);

        IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, TRelaction>> property, Expression<Func<TRelaction, TKey>> relectionKey, Action<T, TRelaction> action);

        IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, ICollection<TRelaction>>> property, Expression<Func<T, TKey>> tkey, Expression<Func<TRelaction, TKey>> relectionKey, Action<T, IEnumerable<TRelaction>> action);

        IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, ICollection<TRelaction>>> property, Expression<Func<T, TKey>> tkey, Action<T, IEnumerable<TRelaction>> action);

        IQuery<T> Include<TRelaction, TKey>(Expression<Func<T, ICollection<TRelaction>>> property, Expression<Func<TRelaction, TKey>> relectionKey, Action<T, IEnumerable<TRelaction>> action);
    }

    /// <summary>
    /// 可执行的Sql接口
    /// </summary>
    public interface IExecutable
    {
        /// <summary>
        /// 执行Sql命令
        /// </summary>
        /// <returns>返回受影响的行数</returns>
        int Execute();
    }
}