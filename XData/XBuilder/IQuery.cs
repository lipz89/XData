using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using XData.Common;

namespace XData.XBuilder
{
    /// <summary>
    /// 查询接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
        /// <param name="expression"></param>
        /// <returns></returns>
        IQuery<T> Where(Expression<Func<T, bool>> expression);

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        IQuery<T> WhereOr(Expression<Func<T, bool>> expression);

        /// <summary>
        /// 清除查询条件
        /// </summary>
        /// <returns></returns>
        IQuery<T> ClearWhere();

        /// <summary>
        /// 排序条件
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        IQuery<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> expression, bool isAsc = true);

        /// <summary>
        /// 多字段升序
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        IQuery<T> OrderBy(params Expression<Func<T, object>>[] expressions);

        /// <summary>
        /// 多字段降序
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        IQuery<T> OrderByDescending(params Expression<Func<T, object>>[] expressions);

        /// <summary>
        /// 清除排序条件
        /// </summary>
        /// <returns></returns>
        IQuery<T> ClearOrder();

        /// <summary>
        /// 设置查询的条数
        /// </summary>
        /// <param name="top">大于0表示查询条数，否则表示查询所有</param>
        /// <returns></returns>
        IQuery<T> Top(int top);

        /// <summary>
        /// 返回非重复记录
        /// </summary>
        /// <param name="distinct">true表示返回非重复数据，否则表示返回所有数据</param>
        /// <returns>查询构建器</returns>
        IQuery<T> Distinct(bool distinct = true);

        /// <summary>
        /// 投影
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> result);

        /// <summary>
        /// 计算查询结果的记录数
        /// </summary>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// 查询结果中的最大值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        TResult Max<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 查询结果中的最小值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        TResult Min<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 查询结果中所有值的和
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        TResult Sum<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 查询结果中的平均值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        TResult Avg<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// 将查询结果输出到列表中
        /// </summary>
        /// <returns></returns>
        List<T> ToList();

        /// <summary>
        /// 提取查询的分页数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Page<T> ToPage(int pageIndex, int pageSize);

        /// <summary>
        /// 提取查询的分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Page<T> ToPage(Page page);

        /// <summary>
        /// 转换成Sql语句
        /// </summary>
        /// <returns></returns>
        string ToSql();
    }
}