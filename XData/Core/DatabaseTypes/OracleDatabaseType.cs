
using System;
using System.Data;


namespace XData.Core.DatabaseTypes
{
    /// <summary>
    /// Oracle 数据源
    /// </summary>
    class OracleDatabaseType : DatabaseType
    {
        /// <summary>
        /// 获取 SQL 参数名称前缀
        /// </summary>
        /// <param name="ConnectionString">数据源连接字符串</param>
        /// <returns>参数名称前缀</returns>
        public override string GetParameterPrefix(string ConnectionString)
        {
            return ":";
        }

        ///// <summary>
        ///// 在命令执行前对命令进行修改
        ///// </summary>
        ///// <param name="cmd">命令</param>
        //public override void PreExecute(IDbCommand cmd)
        //{
        //    cmd.GetType().GetProperty("BindByName").FastSetValue(cmd, true);
        //}

        /// <summary>
        /// 转码标识符
        /// </summary>
        /// <param name="str">要转码的表名或列名</param>
        /// <returns>转码后的表名或列名</returns>
        public override string EscapeSqlIdentifier(string str)
        {
            if (str[0] == '"' && str[str.Length - 1] == '"') return str;

            return string.Format("\"{0}\"", str/*.ToUpperInvariant()*/);
        }

        ///// <summary>
        ///// 返回一个 SQL 表达式，以用来填充自增主键的字段
        ///// </summary>
        ///// <param name="primaryKey">数据表信息</param>
        ///// <returns>一个 SQL 表达式</returns>
        ///// <remarks>参照 Oracle 数据库的相关用法</remarks>
        //public override string GetAutoIncrementExpression(string primaryKey)
        //{
        //    if (!string.IsNullOrEmpty(primaryKey))
        //        return string.Format("{0}.nextval", primaryKey);

        //    return null;
        //}

        ///// <summary>
        ///// 执行插入操作
        ///// </summary>
        ///// <param name="db">数据库对象</param>
        ///// <param name="cmd">要执行插入的命令</param>
        ///// <param name="PrimaryKeyName">主键名</param>
        ///// <returns>插入后的主键值</returns>
        //public override object ExecuteInsert(Database db, IDbCommand cmd, string PrimaryKeyName)
        //{
        //	if(PrimaryKeyName != null)
        //	{
        //		cmd.CommandText += string.Format(" returning {0} into :newid", EscapeSqlIdentifier(PrimaryKeyName));
        //		var param = cmd.CreateParameter();
        //		param.ParameterName = ":newid";
        //		param.Value = DBNull.Value;
        //		param.Direction = ParameterDirection.ReturnValue;
        //		param.DbType = DbType.Int64;
        //		cmd.Parameters.Add(param);
        //		db.ExecuteNonQueryHelper(cmd);
        //		return param.Value;
        //	}
        //	else
        //	{
        //		db.ExecuteNonQueryHelper(cmd);
        //		return -1;
        //	}
        //}
    }
}