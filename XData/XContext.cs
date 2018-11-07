using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using XData.Common;
using XData.Core;
using XData.Extentions;

namespace XData
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public sealed partial class XContext : IDisposable
    {
        private Action<string> sqlLog;
        private bool isDisposed = false;

        #region Properties

        /// <summary>
        /// 获取ADO.NET数据库访问类创建工厂
        /// </summary>
        public DbProviderFactory DbProviderFactory { get; private set; }

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
        internal DatabaseType DatabaseType { get; private set; }

        /// <summary>
        /// sql语句监视方法
        /// </summary>
        public Action<string> SqlLog
        {
            get { return sqlLog; }
            set
            {
                if (value != null)
                {
                    value.Invoke(string.Format("{0} [{1}] 开始日志输出。", DateTime.Now, this.GetHashCode()));
                    //value.Invoke(string.Format("[{0}] 连接字符串: {1}", this.GetHashCode(), this.ConnectionString));
                    //value.Invoke(string.Format("[{0}] 提供程序: {1}", this.GetHashCode(), this.ProviderName));
                }
                else
                {
                    sqlLog?.Invoke(string.Format("{0} [{1}] 停止日志输出。", DateTime.Now, this.GetHashCode()));
                }

                sqlLog = value;
            }
        }

        private XTransaction Transaction { get; set; }

        #endregion

        #region Constructors

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
        /// <summary>
        /// 初始化数据库上下文
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="providerName">数据库连接提供程序</param>
        /// <param name="inTranscation"></param>
        public XContext(string connectionString, string providerName, IsolationLevel inTranscation = IsolationLevel.ReadCommitted)
            : this(connectionString, providerName)
        {
            var conn = this.CreateConnection();
            this.Transaction = new XTransaction(conn, inTranscation);
        }

        #endregion

        #region internal methods

        /// <summary>
        /// 创建数据库连接类
        /// </summary>
        /// <returns>返回数据库连接类</returns>
        private DbConnection CreateConnection()
        {
            if (this.Transaction != null)
            {
                return this.Transaction.Connection;
            }
            DbConnection dbConnection = null;
            if (this.DbProviderFactory != null)
            {
                dbConnection = this.DbProviderFactory.CreateConnection();
                dbConnection.ConnectionString = this.ConnectionString;
                this.sqlLog?.Invoke(string.Format("{0} [{1}] 创建新的连接[{2}]。",
                    DateTime.Now, this.GetHashCode(), dbConnection.GetHashCode()));
            }
            return dbConnection;
        }

        /// <summary>
        /// 创建数据库命令执行类
        /// </summary>
        /// <returns>返回数据库命令执行类</returns>
        private DbCommand CreateCommand()
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
        private DbDataAdapter CreateDataAdapter()
        {
            DbDataAdapter result = null;
            if (this.DbProviderFactory != null)
            {
                result = this.DbProviderFactory.CreateDataAdapter();
            }
            return result;
        }

        private void CloseConnection(DbConnection dbConnection)
        {
            if (this.Transaction != null)
            {
                return;
            }
            if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
            {
                dbConnection.Close();
                dbConnection.Dispose();
                this.sqlLog?.Invoke(string.Format("{0} [{1}] 释放连接[{2}]。", DateTime.Now, this.GetHashCode(), dbConnection.GetHashCode()));
            }
        }

        #endregion

        #region Parameters

        /// <summary>
        /// 创建用来向 <see cref="T:System.Data.IDbCommand" /> 对象表示一个参数类
        /// </summary>
        /// <returns>返回用来向 <see cref="T:System.Data.IDbCommand" /> 对象表示一个参数类</returns>
        private DbParameter CreateParameter()
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
        private DbParameter CreateParameter(string name)
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
        private DbParameter CreateParameter(string name, object value)
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
        public int ExecuteNonQuery(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            this.Check();
            int result = 0;
            DbConnection dbConnection = CreateConnection();
            using (DbCommand dbCommand = this.CreateCommand())
            {
                dbCommand.Connection = dbConnection;
                dbCommand.CommandType = commandType;
                dbCommand.CommandText = sql;
                var pars = CheckNullParameter(parameters);
                if (!pars.IsNullOrEmpty())
                {
                    dbCommand.Parameters.AddRange(pars);
                }
                this.LogFormatter(sql, commandType, pars);
                try
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbConnection.Open();
                    }
                    result = dbCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw NewException(ex, sql, pars, commandType);
                }
                finally
                {
                    CloseConnection(dbConnection);
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
            this.Check();
            int num = 0;
            DbConnection dbConnection = CreateConnection();
            using (DbCommand dbCommand = this.CreateCommand())
            {
                dbCommand.Connection = dbConnection;
                dbCommand.CommandType = CommandType.Text;
                //var log = string.Join(Environment.NewLine, sqls);
                string currentSql = null;
                try
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbConnection.Open();
                    }
                    foreach (string current in sqls)
                    {
                        currentSql = current;
                        dbCommand.CommandText = current;
                        num += dbCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw NewException(ex, currentSql);
                }
                finally
                {
                    CloseConnection(dbConnection);
                }
            }

            return num;
        }

        /// <summary>
        /// 对连接对象执行 SQL 语句
        /// </summary>
        /// <param name="sqls">SQL查询语句和参数对</param> 
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(IEnumerable<KeyValuePair<string, DbParameter[]>> sqls)
        {
            this.Check();
            int num = 0;
            DbConnection dbConnection = CreateConnection();
            using (DbCommand dbCommand = this.CreateCommand())
            {
                dbCommand.Connection = dbConnection;
                dbCommand.CommandType = CommandType.Text;
                string currentSql = null;
                try
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbConnection.Open();
                    }
                    foreach (var current in sqls)
                    {
                        currentSql = current.Key;
                        dbCommand.CommandText = currentSql;
                        var pars = CheckNullParameter(current.Value);
                        this.LogFormatter(currentSql, CommandType.Text, pars);
                        if (!pars.IsNullOrEmpty())
                        {
                            dbCommand.Parameters.AddRange(pars);
                        }
                        num += dbCommand.ExecuteNonQuery();
                        dbCommand.Parameters.Clear();
                    }
                }
                catch (Exception ex)
                {
                    throw NewException(ex, currentSql);
                }
                finally
                {
                    CloseConnection(dbConnection);
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
        public object ExecuteScalar(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            this.Check();
            object result = null;
            DbConnection dbConnection = CreateConnection();
            using (DbCommand dbCommand = this.CreateCommand())
            {
                dbCommand.Connection = dbConnection;
                dbCommand.CommandType = commandType;
                dbCommand.CommandText = sql;
                var pars = CheckNullParameter(parameters);
                if (!pars.IsNullOrEmpty())
                {
                    dbCommand.Parameters.AddRange(pars);
                }
                this.LogFormatter(sql, commandType, pars);
                try
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbConnection.Open();
                    }
                    result = dbCommand.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw NewException(ex, sql, pars, commandType);
                }
                finally
                {
                    CloseConnection(dbConnection);
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
        public DataSet GetDataSet(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            this.Check();
            DataSet dataSet = new DataSet();
            DbConnection dbConnection = CreateConnection();
            using (DbCommand dbCommand = this.CreateCommand())
            {
                dbCommand.Connection = dbConnection;
                dbCommand.CommandType = commandType;
                dbCommand.CommandText = sql;
                var pars = CheckNullParameter(parameters);
                if (!pars.IsNullOrEmpty())
                {
                    dbCommand.Parameters.AddRange(pars);
                }

                this.LogFormatter(sql, commandType, pars);
                using (DbDataAdapter dbDataAdapter = this.CreateDataAdapter())
                {
                    dbDataAdapter.SelectCommand = dbCommand;
                    try
                    {
                        dbDataAdapter.Fill(dataSet);
                    }
                    catch (Exception ex)
                    {
                        throw NewException(ex, sql, pars, commandType);
                    }
                    for (int j = 0; j < dataSet.Tables.Count; j++)
                    {
                        if (string.IsNullOrWhiteSpace(dataSet.Tables[j].TableName))
                        {
                            dataSet.Tables[j].TableName = string.Format("Table{0}", j);
                        }
                    }
                }
                CloseConnection(dbConnection);
            }
            return dataSet;
        }
        #endregion

        #region GetDataTable
        
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
        public DataTable GetDataTable(string sql, CommandType commandType = CommandType.Text, params DbParameter[] parameters)
        {
            this.Check();
            DataTable dataTable = new DataTable();
            DbConnection dbConnection = CreateConnection();
            using (DbCommand dbCommand = this.CreateCommand())
            {
                dbCommand.Connection = dbConnection;
                dbCommand.CommandType = commandType;
                dbCommand.CommandText = sql;
                var pars = CheckNullParameter(parameters);
                if (!pars.IsNullOrEmpty())
                {
                    dbCommand.Parameters.AddRange(pars);
                }

                using (DbDataAdapter dbDataAdapter = this.CreateDataAdapter())
                {
                    dbDataAdapter.SelectCommand = dbCommand;
                    this.LogFormatter(sql, commandType, pars);
                    try
                    {
                        dbDataAdapter.Fill(dataTable);
                    }
                    catch (Exception ex)
                    {
                        throw NewException(ex, sql, pars, commandType);
                    }
                    if (string.IsNullOrWhiteSpace(dataTable.TableName))
                    {
                        dataTable.TableName = "Table1";
                    }
                }
                CloseConnection(dbConnection);
            }
            return dataTable;
        }
        #endregion

        #region Log

        /// <summary>
        ///
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="commandType">查询类型</param>
        /// <param name="parameters">执行参数</param>
        /// <returns></returns>
        private void LogFormatter(string sql, CommandType commandType, DbParameter[] parameters)
        {
            List<string> list = new List<string>();
            //list.Add(string.Format("ConnectionString:{0}", this.ConnectionString));
            //list.Add(string.Format("ProviderName:{0}", this.ProviderName));
            list.Add(string.Format("CommandType [{0}] :", commandType));
            //list.Add("SQL：");
            list.Add(sql);
            if (parameters != null && parameters.Length != 0)
            {
                list.Add("DbParameter：");
                for (int i = 0; i < parameters.Length; i++)
                {
                    DbParameter dbParameter = parameters[i];
                    var value = dbParameter.Value;
                    if (value == null || value == DBNull.Value)
                    {
                        value = "NULL";
                    }
                    else if (value is string)
                    {
                        value = string.Format("'{0}'", value);
                    }
                    list.Add(string.Format("ParameterName：{0,-16}DbType：{1,-16}Value：{2}", dbParameter.ParameterName, dbParameter.DbType, value));
                }
            }
            var sqlString = string.Join(Environment.NewLine, list);
            this.sqlLog?.Invoke(sqlString + Environment.NewLine);
        }

        private XDataException NewException(Exception innerException, string sql, DbParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            var ex = new XDataException(innerException)
            {
                Parameters = parameters?.ToArray(),
                ConnectionString = this.ConnectionString,
                ProviderName = this.ProviderName,
                CommandType = commandType,
                SqlString = sql
            };
            this.Transaction?.AddException(ex);
            return ex;
        }

        #endregion

        #region Dispose

        private void Check()
        {
            if (isDisposed)
            {
                throw Error.Exception("对象资源已释放，无法继续使用。");
            }
        }

        private void Dispose(bool disposing)
        {
            //释放非托管资源   
            this.DatabaseType = null;
            this.DbProviderFactory = null;
            this.sqlLog = null;
            this.isDisposed = true;

            this.Transaction?.Dispose();
            this.Transaction = null;

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// 释放<see cref="XContext" />的非托管资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~XContext()
        {
            Dispose(false);
        }

        #endregion
    }
}