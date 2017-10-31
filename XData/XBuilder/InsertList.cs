using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

using XData.Extentions;

namespace XData.XBuilder
{
    internal sealed class InsertList<T> : IExecutable
    {
        private readonly XContext context;
        private readonly IEnumerable<Insert<T>> inserts;

        private InsertList(XContext context)
        {
            this.context = context;
        }

        internal InsertList(XContext context, IEnumerable<T> entities) : this(context)
        {
            inserts = entities?.Select(x => new Insert<T>(context, x));
        }

        internal InsertList(XContext context, IEnumerable<T> entities, bool include, Expression<Func<T, object>>[] fields) : this(context)
        {
            inserts = entities?.Select(x => new Insert<T>(context, x, include, fields));
        }

        internal InsertList(XContext context, IEnumerable<IDictionary<string, object>> fieldValues)
        {
            inserts = fieldValues?.Select(x => new Insert<T>(context, x));
        }

        public int Execute()
        {
            var pairs = inserts?.Select(x => new KeyValuePair<string, DbParameter[]>(x.ToSql(), x.DbParameters)).ToList();
            if (pairs.IsNullOrEmpty())
            {
                return 0;
            }
            return context.ExecuteNonQuery(pairs);
        }
    }
}