using System;
using System.Data;

using XData.Common;
using XData.Core.DatabaseTypes;
using XData.Extentions;

namespace XData.Core
{
    /// <summary>
    /// 数据源类型基类
    /// </summary>
    public abstract class DatabaseType
    {
        /// <summary>
        /// 获取 SQL 参数名称前缀
        /// </summary>
        /// <returns>参数名称前缀</returns>
        public virtual string GetParameterPrefix(string connectionString)
        {
            return "@";
        }

        /// <summary>
        /// 将 C# 数据类型转换为相应数据源的数据类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的值</returns>
        public virtual object MapParameterValue(object value)
        {
            if (value is bool b)
            {
                return b ? 1 : 0;
            }

            return value;
        }

        /// <summary>
        /// 在命令执行前对命令进行修改
        /// </summary>
        /// <param name="cmd">命令</param>
        public virtual void PreExecute(IDbCommand cmd) { }

        /// <summary>
        /// 返回用于查询记录是否存在的 SQL 语句
        /// </summary>
        /// <returns>用于查询记录是否存在的 SQL 语句</returns>
        public virtual string GetExistsSql()
        {
            return "SELECT COUNT(*) FROM {0} WHERE {1}";
        }

        /// <summary>
        /// 转码表名
        /// </summary>
        /// <param name="tableName">要转码的表名</param>
        /// <returns>转码后的表名</returns>
        public virtual string EscapeTableName(string tableName)
        {
            return tableName.IndexOf('.') >= 0 ? tableName : EscapeSqlIdentifier(tableName);
        }

        /// <summary>
        /// 转码标识符
        /// </summary>
        /// <param name="str">要转码的表名或列名</param>
        /// <returns>转码后的表名或列名</returns>
        public virtual string EscapeSqlIdentifier(string str)
        {
            if (str[0] == '[' && str[str.Length - 1] == ']')
            {
                return str;
            }
            return $"[{str}]";
        }

        /// <summary>
        /// 返回一个 SQL 表达式，以用来填充自增主键的返回值
        /// </summary>
        /// <param name="primaryKeyName">主键名</param>
        /// <returns>一个 SQL 表达式</returns>
        /// <remarks>参照 MS SQLServer 数据库的相关用法</remarks>
        public virtual string GetInsertOutputClause(string primaryKeyName)
        {
            return string.Empty;
        }

        /// <summary>
        /// 返回当前正在使用的数据源类型
        /// </summary>
        /// <param name="typeName">类型名</param>
        /// <param name="providerName">适配器名</param>
        /// <returns>数据源类型</returns>
        public static DatabaseType Resolve(string typeName, string providerName)
        {
            //Try using type name first(more reliable)
            if (typeName.StartsWith("MySql"))
                return Singleton<MySqlDatabaseType>.Instance;
            if (typeName.StartsWith("SqlCe"))
                return Singleton<SqlServerCEDatabaseType>.Instance;
            if (typeName.StartsWith("Npgsql") || typeName.StartsWith("PgSql"))
                return Singleton<PostgreSQLDatabaseType>.Instance;
            if (typeName.StartsWith("Oracle"))
                return Singleton<OracleDatabaseType>.Instance;
            if (typeName.StartsWith("SQLite"))
                return Singleton<SqliteDatabaseType>.Instance;
            if (typeName.StartsWith("System.Data.SqlClient.") || typeName.StartsWith("SqlConnection"))
            {
                return Singleton<SqlServerDatabaseType>.Instance;
            }

            if (!providerName.IsNullOrWhiteSpace())
            {
                //Try again with provider name
                if (providerName.IndexOf("MySql", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return Singleton<MySqlDatabaseType>.Instance;
                if (providerName.IndexOf("SqlServerCe", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return Singleton<SqlServerCEDatabaseType>.Instance;
                if (providerName.IndexOf("pgsql", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return Singleton<PostgreSQLDatabaseType>.Instance;
                if (providerName.IndexOf("Oracle", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return Singleton<OracleDatabaseType>.Instance;
                if (providerName.IndexOf("SQLite", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return Singleton<SqliteDatabaseType>.Instance;
            }
            // Assume SQL Server
            return Singleton<SqlServerDatabaseType>.Instance;
        }
    }
}