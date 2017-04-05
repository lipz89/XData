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
    }
}