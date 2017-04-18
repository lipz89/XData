using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using XData.Common;

namespace XData
{
    /// <summary>
    /// 事务封装对象
    /// </summary>
    internal class XTransaction : IDisposable
    {
        private readonly List<Exception> exceptions = new List<Exception>();
        /// <summary>
        /// 事务对象
        /// </summary>
        public DbTransaction Transaction { get; internal set; }
        /// <summary>
        /// 事务所依附的数据库连接
        /// </summary>
        public DbConnection Connection { get; internal set; }
        /// <summary>
        /// 事务过程中发生的异常
        /// </summary>
        public IReadOnlyList<Exception> Exceptions => exceptions.AsReadOnly();
        /// <summary>
        /// 事务状态
        /// </summary>
        public TransactionState State { get; private set; } = TransactionState.None;

        /// <summary>
        /// 实例化一个事务对象。
        /// </summary>
        /// <param name="context">事务对象作用的连接上下文。</param>
        public XTransaction(XContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNullException(nameof(context));
            }
            this.Init(context.CreateConnection());
            context.Transaction = this;
        }

        private void Init(DbConnection dbConnection)
        {
            if (dbConnection != null)
            {
                if (dbConnection.State == ConnectionState.Broken)
                {
                    dbConnection.Close();
                }
                if (dbConnection.State == ConnectionState.Closed)
                {
                    dbConnection.Open();
                }
                Connection = dbConnection;
                Transaction = dbConnection.BeginTransaction();
                State = TransactionState.Inited;
            }
            else
            {
                throw Error.ArgumentNullException(nameof(dbConnection));
            }
        }

        /// <summary>
        /// 提交事务。
        /// </summary>
        public void Commit()
        {
            if (Transaction != null && State == TransactionState.Inited)
            {
                Transaction.Commit();
                State = TransactionState.Committed;
            }
        }

        /// <summary>
        /// 回滚事务。
        /// </summary>
        public void Rollback()
        {
            if (Transaction != null && State == TransactionState.Inited)
            {
                Transaction.Rollback();
                State = TransactionState.Rollbacked;
            }
        }

        /// <summary>
        /// 完成事务，如果没有异常提交事务，否则回滚事务。
        /// </summary>
        public void Complete()
        {
            if (exceptions.Any())
            {
                this.Rollback();
            }
            else
            {
                this.Commit();
            }
        }

        /// <summary>
        /// 添加一个异常
        /// </summary>
        /// <param name="exception"></param>
        public void AddException(Exception exception)
        {
            exceptions.Add(exception);
        }

        /// <summary>
        /// 释放<see cref="XTransaction"/>的非托管资源。
        /// </summary>
        public void Dispose()
        {
            if (this.State != TransactionState.None)
            {
                this.Complete();
                this.Transaction?.Dispose();
                this.Connection?.Dispose();
                this.Transaction = null;
                this.Connection = null;
                this.exceptions.Clear();
                this.State = TransactionState.None;
            }
        }
    }

    /// <summary>
    /// 事务状态
    /// </summary>
    internal enum TransactionState
    {
        /// <summary>
        /// 无状态
        /// </summary>
        None,
        /// <summary>
        /// 已初始化
        /// </summary>
        Inited,
        /// <summary>
        /// 已提交
        /// </summary>
        Committed,
        /// <summary>
        /// 已回滚
        /// </summary>
        Rollbacked
    }
}