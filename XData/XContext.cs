using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using XData.Common;
using XData.Core;
using XData.Extentions;

namespace XData
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public partial class XContext
    {
        #region Properties

        /// <summary>
        /// 获取ADO.NET数据库访问类创建工厂
        /// </summary>
        public DbProviderFactory DbProviderFactory { get; }

        /// <summary>
        /// 数据库连接提供程序
        /// </summary>
        public string ProviderName { get; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        internal DatabaseType DatabaseType { get; set; }

        #endregion

        #region Constructors

        ///// <summary>
        ///// 初始化数据库上下文
        ///// </summary>
        ///// <param name="config">数据库连接名称</param>
        //public XContext(DbConfig config)
        //{
        //    if (config == null)
        //    {
        //        throw Error.ArgumentNullException(nameof(config));
        //    }

        //    if (string.IsNullOrWhiteSpace(config.ConnectionString))
        //    {
        //        throw Error.Exception(string.Format("未获取到{0}的数据库链接的连接字符串。", config.Name));
        //    }
        //    if (string.IsNullOrWhiteSpace(config.ProviderName))
        //    {
        //        throw Error.Exception(string.Format("未获取到{0}的数据库链接的提供程序。", config.Name));
        //    }
        //    this.ProviderName = config.ProviderName;
        //    this.DbProviderFactory = DbProviderFactories.GetFactory(this.ProviderName);
        //    this.ConnectionString = config.ConnectionString;
        //}

        /// <summary>
        /// 初始化数据库上下文
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="providerName">数据库连接提供程序</param>
        public XContext(string connectionString, string providerName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw Error.ArgumentNullException(nameof(connectionString));
            }
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw Error.ArgumentNullException(nameof(providerName));
            }
            ProviderName = providerName;
            DbProviderFactory = DbProviderFactories.GetFactory(this.ProviderName);
            ConnectionString = connectionString;
            DatabaseType = DatabaseType.Resolve(DbProviderFactory.GetType().FullName, ProviderName);
        }
        #endregion

        #region internal methods

        /// <summary>
        /// 创建数据库连接类
        /// </summary>
        /// <returns>返回数据库连接类</returns>
        internal DbConnection CreateConnection()
        {
            DbConnection dbConnection = null;
            if (this.DbProviderFactory != null)
            {
                dbConnection = this.DbProviderFactory.CreateConnection();
                dbConnection.ConnectionString = this.ConnectionString;
            }
            return dbConnection;
        }

        /// <summary>
        /// 创建数据库命令执行类
        /// </summary>
        /// <returns>返回数据库命令执行类</returns>
        internal DbCommand CreateCommand()
        {
            DbCommand dbCommand = null;
            if (this.DbProviderFactory != null)
            {
                dbCommand = this.DbProviderFactory.CreateCommand();
                dbCommand.CommandTimeout = 6000;
            }
            return dbCommand;
        }

        /// <summary>
        /// 创建用于填充<see cref="T:System.Data.DataSet" />和更新数据源类
        /// </summary>
        /// <returns>返回用于填充<see cref="T:System.Data.DataSet" />和更新数据源类</returns>
        internal DbDataAdapter CreateDataAdapter()
        {
            DbDataAdapter result = null;
            if (this.DbProviderFactory != null)
            {
                result = this.DbProviderFactory.CreateDataAdapter();
            }
            return result;
        }
        #endregion

        #region Parameters

        /// <summary>
        /// 创建用来向 <see cref="T:System.Data.IDbCommand" /> 对象表示一个参数类
        /// </summary>
        /// <returns>返回用来向 <see cref="T:System.Data.IDbCommand" /> 对象表示一个参数类</returns>
        public DbParameter CreateParameter()
        {
            DbParameter result = null;
            if (this.DbProviderFactory != null)
            {
                result = this.DbProviderFactory.CreateParameter();
            }
            return result;
        }

        /// <summary>
        /// 创建用来向 <see cref="T:System.Data.IDbCommand" /> 对象表示一个参数类
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>返回用来向 <see cref="T:System.Data.IDbCommand" /> 对象表示一个参数类</returns>
        public DbParameter CreateParameter(string name)
        {
            DbParameter parameter = this.CreateParameter();
            parameter.ParameterName = name;
            return parameter;
        }

        /// <summary>
        /// 创建用来向 <see cref="T:System.Data.IDbCommand" /> 对象表示一个参数类
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>返回用来向 <see cref="T:System.Data.IDbCommand" /> 对象表示一个参数类</returns>
        public DbParameter CreateParameter(string name, object value)
        {
            DbParameter parameter = this.CreateParameter(name);
            parameter.Value = value;
            return parameter;
        }

        /// <summary>
        /// 创建用来向 <see cref="T:System.Data.IDbCommand" /> 对象表示一个参数类
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="type">参数类型</param>
        /// <param name="value">参数值</param>
        /// <returns>返回用来向 <see cref="T:System.Data.IDbCommand" /> 对象表示一个参数类</returns>
        public DbParameter CreateParameter(string name, DbType type, object value)
        {
            DbParameter parameter = this.CreateParameter(name);
            parameter.DbType = type;
            parameter.Value = value;
            return parameter;
        }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// 对连接对象执行 SQL 语句
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(string sql)
        {
            return this.ExecuteNonQuery(sql, CommandType.Text);
        }

        /// <summary>
        /// 对连接对象执行 SQL 语句
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(string sql, CommandType commandType)
        {
            return this.ExecuteNonQuery(sql, commandType, null);
        }

        /// <summary>
        /// 对连接对象执行 SQL 语句
        /// </summary>
        /// <param name="sql">SQL查询语句</param> 
        /// <param name="parameters">执行参数</param>
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(string sql, params DbParameter[] parameters)
        {
            return this.ExecuteNonQuery(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// 对连接对象执行 SQL 语句
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(string sql, CommandType commandType, params DbParameter[] parameters)
        {
            int result = 0;
            using (DbConnection dbConnection = this.CreateConnection())
            {
                using (DbCommand dbCommand = this.CreateCommand())
                {
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandType = commandType;
                    dbCommand.CommandText = sql;
                    if (!parameters.IsNullOrEmpty())
                    {
                        dbCommand.Parameters.AddRange(CheckNullParameter(parameters));
                    }
                    var log = this.LogFormatter(new List<string> { sql }, commandType, parameters);
                    try
                    {
                        dbConnection.Open();
                        result = dbCommand.ExecuteNonQuery();
                        this.Log(LogLevel.Information, string.Format("SQL语句执行ExecuteNonQuery成功！{0}{1}", Environment.NewLine, log), null);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("SQL语句执行失败！{0}{1}", Environment.NewLine, log);
                        this.Log(LogLevel.Error, message, ex);
                        throw;
                    }
                    finally
                    {
                        if (dbConnection.State != ConnectionState.Closed)
                        {
                            dbConnection.Close();
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 对连接对象执行 SQL 语句
        /// </summary>
        /// <param name="sqls">SQL查询语句集合</param>
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(IList<string> sqls)
        {
            int num = 0;
            using (DbConnection dbConnection = this.CreateConnection())
            {
                using (DbCommand dbCommand = this.CreateCommand())
                {
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandType = CommandType.Text;
                    DbTransaction dbTransaction = null;
                    var log = this.LogFormatter(sqls, CommandType.Text, null);
                    try
                    {
                        dbConnection.Open();
                        dbTransaction = dbConnection.BeginTransaction();
                        foreach (string current in sqls)
                        {
                            dbCommand.CommandText = current;
                            dbCommand.Transaction = dbTransaction;
                            num += dbCommand.ExecuteNonQuery();
                        }
                        dbTransaction.Commit();
                        this.Log(LogLevel.Information, string.Format("SQL语句执行ExecuteNonQuery成功！{0}{1}", Environment.NewLine, log), null);
                    }
                    catch (Exception ex)
                    {
                        if (dbTransaction != null)
                        {
                            dbTransaction.Rollback();
                        }
                        string message = string.Format("SQL语句执行失败！{0}{1}", Environment.NewLine, log);
                        this.Log(LogLevel.Error, message, ex);
                        throw;
                    }
                    finally
                    {
                        if (dbConnection.State != ConnectionState.Closed)
                        {
                            dbConnection.Close();
                        }
                    }
                }
            }
            return num;
        }
        #endregion

        #region ExecuteScalar

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <returns>返回结果集中第一行的第一列</returns>
        public object ExecuteScalar(string sql)
        {
            return this.ExecuteScalar(sql, CommandType.Text);
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <returns>返回结果集中第一行的第一列</returns>
        public object ExecuteScalar(string sql, CommandType commandType)
        {
            return this.ExecuteScalar(sql, commandType, null);
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回结果集中第一行的第一列</returns>
        public object ExecuteScalar(string sql, params DbParameter[] parameters)
        {
            return this.ExecuteScalar(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回结果集中第一行的第一列</returns>
        public object ExecuteScalar(string sql, CommandType commandType, params DbParameter[] parameters)
        {
            object result = null;
            using (DbConnection dbConnection = this.CreateConnection())
            {
                using (DbCommand dbCommand = this.CreateCommand())
                {
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandType = commandType;
                    dbCommand.CommandText = sql;
                    if (!parameters.IsNullOrEmpty())
                    {
                        dbCommand.Parameters.AddRange(CheckNullParameter(parameters));
                    }
                    var log = this.LogFormatter(new List<string> { sql }, commandType, parameters);
                    try
                    {
                        dbConnection.Open();
                        result = dbCommand.ExecuteScalar();
                        this.Log(LogLevel.Information, string.Format("SQL语句执行ExecuteScalar成功！{0}{1}", Environment.NewLine, log), null);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("SQL语句执行失败！{0}{1}", Environment.NewLine, log);
                        this.Log(LogLevel.Error, message, ex);
                        throw;
                    }
                    finally
                    {
                        if (dbConnection.State != ConnectionState.Closed)
                        {
                            dbConnection.Close();
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region GetDataSet

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <returns>返回数据缓存</returns>
        public DataSet GetDataSet(string sql)
        {
            return this.GetDataSet(sql, CommandType.Text);
        }

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <returns>返回数据缓存</returns>
        public DataSet GetDataSet(string sql, CommandType commandType)
        {
            return this.GetDataSet(sql, commandType, null);
        }

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回数据缓存</returns>
        public DataSet GetDataSet(string sql, params DbParameter[] parameters)
        {
            return this.GetDataSet(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回数据缓存</returns>
        public DataSet GetDataSet(string sql, CommandType commandType, params DbParameter[] parameters)
        {
            DataSet dataSet = new DataSet();
            using (DbConnection dbConnection = this.CreateConnection())
            {
                using (DbCommand dbCommand = this.CreateCommand())
                {
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandType = commandType;
                    dbCommand.CommandText = sql;
                    if (!parameters.IsNullOrEmpty())
                    {
                        dbCommand.Parameters.AddRange(CheckNullParameter(parameters));
                    }

                    var log = this.LogFormatter(new List<string> { sql }, commandType, parameters);
                    using (DbDataAdapter dbDataAdapter = this.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = dbCommand;
                        try
                        {
                            dbDataAdapter.Fill(dataSet);
                            this.Log(LogLevel.Information, string.Format("SQL语句执行GetDataSet成功！{0}{1}", Environment.NewLine, log), null);
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("SQL语句执行失败！{0}{1}", Environment.NewLine, log);
                            this.Log(LogLevel.Error, message, ex);
                            throw;
                        }
                        for (int j = 0; j < dataSet.Tables.Count; j++)
                        {
                            if (string.IsNullOrWhiteSpace(dataSet.Tables[j].TableName))
                            {
                                dataSet.Tables[j].TableName = string.Format("Table{0}", j);
                            }
                        }
                    }
                }
            }
            return dataSet;
        }
        #endregion

        #region GetDataTable

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <returns>返回数据缓存</returns>
        public DataTable GetDataTable(string sql)
        {
            return this.GetDataTable(sql, CommandType.Text);
        }

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <returns>返回数据缓存</returns>
        public DataTable GetDataTable(string sql, CommandType commandType)
        {
            return this.GetDataTable(sql, commandType, null);
        }

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回数据缓存</returns>
        public DataTable GetDataTable(string sql, params DbParameter[] parameters)
        {
            return this.GetDataTable(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// 从数据源创建数据缓存
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <param name="parameters">执行参数</param>
        /// <returns>返回数据缓存</returns>
        public DataTable GetDataTable(string sql, CommandType commandType, params DbParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            using (DbConnection dbConnection = this.CreateConnection())
            {
                using (DbCommand dbCommand = this.CreateCommand())
                {
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandType = commandType;
                    dbCommand.CommandText = sql;
                    if (!parameters.IsNullOrEmpty())
                    {
                        dbCommand.Parameters.AddRange(CheckNullParameter(parameters));
                    }

                    using (DbDataAdapter dbDataAdapter = this.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = dbCommand;
                        var log = this.LogFormatter(new List<string> { sql }, commandType, parameters);
                        try
                        {
                            dbDataAdapter.Fill(dataTable);
                            this.Log(LogLevel.Information, string.Format("SQL语句执行GetDataTable成功！{0}{1}", Environment.NewLine, log), null);
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("SQL语句执行失败！{0}{1}", Environment.NewLine, log);
                            this.Log(LogLevel.Error, message, ex);
                            throw;
                        }
                        if (string.IsNullOrWhiteSpace(dataTable.TableName))
                        {
                            dataTable.TableName = "Table1";
                        }
                    }
                }
            }
            return dataTable;
        }

        /* 执行三步递交的存储过程
        /// <summary>
        /// 执行三步递交的存储过程
        /// </summary>
        /// <param name="sqls">执行存储过程的SQL字符串数组</param>
        /// <param name="msg">返回的提示信息</param>
        /// <returns>返回是否执行成功</returns>
        public bool ExecuteThreeStepProcedure(IList<string> sqls, out string msg)
        {
            bool result = true;
            msg = "T";
            using (DbConnection dbConnection = this.CreateConnection())
            {
                using (DbCommand dbCommand = this.CreateCommand())
                {
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandType = CommandType.Text;
                    using (DbDataAdapter dbDataAdapter = this.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = dbCommand;
                        try
                        {
                            dbConnection.Open();
                            for (int i = 0; i < sqls.Count; i++)
                            {
                                DataTable dataTable = new DataTable();
                                dbCommand.CommandText = sqls[i];
                                dbDataAdapter.Fill(dataTable);
                                if (i == 0)
                                {
                                    if (dataTable.Rows.Count > 0 && dataTable.Rows[0][0].ToString().Trim() == "F")
                                    {
                                        if (dataTable.Columns.Count > 1 && !string.IsNullOrWhiteSpace(dataTable.Rows[0][1].ToString().Trim()))
                                        {
                                            msg = string.Format("第一步 {0}", dataTable.Rows[0][1].ToString().Trim());
                                        }
                                        else
                                        {
                                            msg = "第一步 执行存储过程意外出错！";
                                        }
                                        result = false;
                                        break;
                                    }
                                    this.Log(LogLevel.Information, string.Format("三步递交第一步执行成功！{0}SQL:{1}", Environment.NewLine, dbCommand.CommandText), null);
                                }
                                else if (i > 0 && i < sqls.Count - 1)
                                {
                                    if (dataTable.Rows.Count > 0 && dataTable.Rows[0][0].ToString().Trim() == "F")
                                    {
                                        if (dataTable.Columns.Count > 1 && !string.IsNullOrWhiteSpace(dataTable.Rows[0][1].ToString().Trim()))
                                        {
                                            msg = string.Format("第二步 {0}", dataTable.Rows[0][1].ToString().Trim());
                                        }
                                        else
                                        {
                                            msg = "第二步 执行存储过程意外出错！";
                                        }
                                        result = false;
                                        break;
                                    }
                                    this.Log(LogLevel.Information, string.Format("三步递交第二步执行成功！{0}SQL:{1}", Environment.NewLine, dbCommand.CommandText), null);
                                }
                                else
                                {
                                    if (dataTable.Rows.Count > 0)
                                    {
                                        if (dataTable.Rows[0][0].ToString().Trim() == "F")
                                        {
                                            if (dataTable.Columns.Count > 1 && !string.IsNullOrWhiteSpace(dataTable.Rows[0][1].ToString().Trim()))
                                            {
                                                msg = string.Format("第三步 {0}", dataTable.Rows[0][1].ToString().Trim());
                                            }
                                            else
                                            {
                                                msg = "第三步 执行存储过程意外出错！";
                                            }
                                            result = false;
                                            break;
                                        }
                                        if (dataTable.Columns.Count > 1 && !string.IsNullOrWhiteSpace(dataTable.Rows[0][1].ToString().Trim()))
                                        {
                                            msg = dataTable.Rows[0][1].ToString().Trim();
                                        }
                                    }
                                    this.Log(LogLevel.Information, string.Format("三步递交第三步执行成功！{0}SQL:{1}", Environment.NewLine, dbCommand.CommandText), null);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            List<string> list = new List<string>();
                            list.Add("执行三步递交的存储过程失败！");
                            foreach (string current in sqls)
                            {
                                list.Add(current);
                            }
                            this.Log(LogLevel.Error, string.Join("\r\n", list), ex);
                            throw;
                        }
                        finally
                        {
                            if (dbConnection.State != ConnectionState.Closed)
                            {
                                dbConnection.Close();
                            }
                        }
                    }
                }
            }
            return result;
        }
        //*/
        #endregion

        #region Log

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">消息</param>
        /// <param name="ex">异常</param>
        private void Log(LogLevel level, string message, Exception ex = null)
        {
            //ConsoleWrite(level, message, ex);
            //Log(level, 0, message, ex, null);
        }

        [Conditional("DEBUG")]
        private void ConsoleWrite(LogLevel level, string message, Exception ex = null)
        {
            Console.WriteLine(level + ":");
            Console.WriteLine(message);
            if (ex != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sqls">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <param name="parameters">执行参数</param>
        /// <returns></returns>
        private string LogFormatter(IList<string> sqls, CommandType commandType, DbParameter[] parameters)
        {
            List<string> list = new List<string>();
            list.Add(string.Format("ConnectionString:{0}", this.ConnectionString));
            list.Add(string.Format("ProviderName:{0}", this.ProviderName));
            list.Add("SQL：");
            foreach (string current in sqls)
            {
                list.Add(current);
            }
            list.Add(string.Format("CommandType：\r\n{0}", commandType));
            if (parameters != null && parameters.Length != 0)
            {
                list.Add("DbParameter：");
                for (int i = 0; i < parameters.Length; i++)
                {
                    DbParameter dbParameter = parameters[i];
                    list.Add(string.Format("ParameterName：{0,-32}DbType：{1,-16}Value：{2}", dbParameter.ParameterName, dbParameter.DbType, dbParameter.Value ?? "NULL"));
                }
            }
            return string.Join("\r\n", list);
        }
        #endregion
    }
}