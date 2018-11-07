namespace XData.Core.DatabaseTypes
{
	/// <summary>
	/// SqlServer CE 数据源
	/// </summary>
	class SqlServerCEDatabaseType : DatabaseType
	{
		///// <summary>
		///// 执行插入操作
		///// </summary>
		///// <param name="db">数据库对象</param>
		///// <param name="cmd">要执行插入的命令</param>
		///// <param name="PrimaryKeyName">主键名</param>
		///// <returns>插入后的主键值</returns>
		//public override object ExecuteInsert(Database db, System.Data.IDbCommand cmd, string PrimaryKeyName)
		//{
		//	db.ExecuteNonQueryHelper(cmd);
		//	return db.ExecuteScalar<object>("SELECT @@@IDENTITY AS NewID;");
		//}
	}
}