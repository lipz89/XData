using System;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace XData.Common
{
    /// <summary>
    /// 异常信息
    /// </summary>
    [Serializable]
    public class XDataException : Exception
    {
        /// <summary>
        /// 引发异常的Sql语句
        /// </summary>
        public string SqlString { get; internal set; }
        /// <summary>
        /// 引发异常的数据库连接字符串
        /// </summary>
        public string ConnectionString { get; internal set; }
        /// <summary>
        /// 引发异常的数据库提供程序
        /// </summary>
        public string ProviderName { get; internal set; }
        /// <summary>
        /// 引发异常的命令类型
        /// </summary>
        public CommandType CommandType { get; internal set; }
        /// <summary>
        /// 引发异常的Sql执行参数
        /// </summary>
        public DbParameter[] Parameters { get; internal set; }

        internal XDataException()
        {
        }

        internal XDataException(string message) : base(message)
        {
        }

        internal XDataException(Exception inner) : base("SQL语句执行失败。", inner)
        {
        }

        internal XDataException(string message, Exception inner) : base(message, inner)
        {
        }

        internal XDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}