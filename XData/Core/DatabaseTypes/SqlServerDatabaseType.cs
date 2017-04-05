
namespace XData.Core.DatabaseTypes
{
    /// <summary>
    /// SQLServer 数据源
    /// </summary>
    class SqlServerDatabaseType : DatabaseType
    {
        /// <summary>
        /// 返回用于查询记录是否存在的 SQL 语句
        /// </summary>
        /// <returns>用于查询记录是否存在的 SQL 语句</returns>
        public override string GetExistsSql()
        {
            return "IF EXISTS (SELECT 1 FROM {0} WHERE {1}) SELECT 1 ELSE SELECT 0";
        }

        /// <summary>
        /// 返回一个 SQL 表达式，以用来填充自增主键的返回值
        /// </summary>
        /// <param name="primaryKeyName">主键名</param>
        /// <returns>一个 SQL 表达式</returns>
        /// <remarks>参照 MS SQLServer 数据库的相关用法</remarks>
        public override string GetInsertOutputClause(string primaryKeyName)
        {
            return string.Format(" OUTPUT INSERTED.[{0}]", primaryKeyName);
        }
    }
}