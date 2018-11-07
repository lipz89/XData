using System;

using XData.Common;

namespace XData.Core
{
    /// <summary>
    /// 可以转换成Sql查询条件的函数
    /// </summary>
    public static class XSqlFunctions
    {
        /// <summary>
        /// 字符串Like查询
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="pattern">匹配表达式</param>
        /// <returns></returns>
        public static bool SqlLike(this string source, string pattern)
        {
            throw Error.NotSupportedException("方法未实现。");
        }
        /// <summary>
        /// Between方法
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="value">目标值</param>
        /// <param name="start">最小值</param>
        /// <param name="end">最大值</param>
        /// <returns></returns>
        public static bool Between<T>(this T value, T start, T end) where T : struct, IComparable<T>
        {
            var gte = value.CompareTo(start) >= 0;
            var lte = value.CompareTo(end) <= 0;
            return gte && lte;
        }
    }
}