﻿using System;

namespace XData.Core.DatabaseTypes
{
    /// <summary>
    /// MySql 数据源
    /// </summary>
    class MySqlDatabaseType : DatabaseType
    {
        /// <summary>
        /// 获取 SQL 参数名称前缀
        /// </summary>
        /// <param name="connectionString">数据源连接字符串</param>
        /// <returns>参数名称前缀</returns>
        public override string GetParameterPrefix(string connectionString)
        {
            if (connectionString != null && connectionString.IndexOf("Allow User Variables=true", StringComparison.Ordinal) >= 0)
                return "?";
            else
                return "@";
        }

        /// <summary>
        /// 转码标识符
        /// </summary>
        /// <param name="str">要转码的表名或列名</param>
        /// <returns>转码后的表名或列名</returns>
        public override string EscapeSqlIdentifier(string str)
        {
            if (str[0] == '`' && str[str.Length - 1] == '`') return str;

            return $"`{str}`";
        }

        /// <summary>
        /// 返回用于查询记录是否存在的 SQL 语句
        /// </summary>
        /// <returns>用于查询记录是否存在的 SQL 语句</returns>
        public override string GetExistsSql()
        {
            return "SELECT EXISTS (SELECT 1 FROM {0} WHERE {1})";
        }
    }
}