using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace XData.XBuilder.Include
{
    internal class IncludePropertyInfo<T, TRelaction, TKey> : IncludeInfo<T, TRelaction, TKey>
    {
        public IncludePropertyInfo(XContext context,
                                   Expression<Func<T, TKey>> key,
                                   Expression<Func<TRelaction, TKey>> relectionKey,
                                   Action<T, TRelaction> action)
            : base(context, key, relectionKey)
        {
            Action = action;
        }

        private Action<T, TRelaction> Action { get; }

        public override List<T> Invoke(List<T> list)
        {
            var keys = list.Select(Key).ToList();
            var lbd = GetWhere(keys);

            var relactions = Context.Query<TRelaction>().Where(lbd).ToList();

            foreach (var var in list)
            {
                var k = Key(var);

                var rel = relactions.FirstOrDefault(x => RelectionKey(x).Equals(k));
                if (rel != null)
                {
                    Action(var, rel);
                }
            }

            return list;
        }

        public override T Invoke(T item)
        {
            var key = Key(item);
            var lbd = GetWhere(key);

            var relaction = Context.Query<TRelaction>().Where(lbd).FirstOrDefault();

            if (relaction != null)
            {
                Action(item, relaction);
            }

            return item;
        }
    }
}