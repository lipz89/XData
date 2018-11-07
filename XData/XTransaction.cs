using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace XData
{
    internal class XTransaction : IDisposable
    {
        public XTransaction(DbConnection connection, IsolationLevel isolationLevel)
        {
            Connection = connection;
            Transaction = connection.BeginTransaction(isolationLevel);
            Exceptions = new List<Exception>();
        }
        public List<Exception> Exceptions { get; }
        public DbTransaction Transaction { get; }
        public DbConnection Connection { get; }
        public void Commit()
        {
            Transaction.Commit();
        }

        public void Rollback()
        {
            Transaction.Rollback();
        }

        public void AddException(Exception ex)
        {
            this.Exceptions.Add(ex);
        }

        public void Dispose()
        {
            Transaction.Dispose();
            Connection.Dispose();
        }
    }
}