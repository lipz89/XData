﻿namespace XData.Core.DatabaseTypes
{
    /// <summary>
    /// SQLite 数据源
    /// </summary>
    class SqliteDatabaseType : DatabaseType
    {
        /// <summary>
        /// 将 C# 数据类型转换为相应数据源的数据类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>转换后的值</returns>
        public override object MapParameterValue(object value)
        {
            if (value is uint u)
                return (long)u;

            return base.MapParameterValue(value);
        }

        ///// <summary>
        ///// 执行插入操作
        ///// </summary>
        ///// <param name="db">数据库对象</param>
        ///// <param name="cmd">要执行插入的命令</param>
        ///// <param name="PrimaryKeyName">主键名</param>
        ///// <returns>插入后的主键值</returns>
        //public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string PrimaryKeyName)
        //{
        //	if (PrimaryKeyName != null)
        //	{
        //		cmd.CommandText += ";\nSELECT last_insert_rowid();";
        //		return db.ExecuteScalarHelper(cmd);
        //	}
        //	else
        //	{
        //		db.ExecuteNonQueryHelper(cmd);
        //		return -1;
        //	}
        //}

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