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
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool SqlLike(this string source, string pattern)
        {
            throw Error.NotSupportedException("方法未实现。");
        }
        /// <summary>
        /// Between方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool Between<T>(this T value, T start, T end) where T : struct, IComparable<T>
        {
            var iss = value.CompareTo(start) >= 0;
            var ise = value.CompareTo(end) <= 0;
            return iss && ise;
        }
    }
}