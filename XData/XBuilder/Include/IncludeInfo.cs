using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using XData.Common;
using XData.Extentions;

namespace XData.XBuilder
{
    internal abstract class IncludeInfo<T, TRelaction, TKey> : IInclude<T>
    {
        protected IncludeInfo(XContext context, Expression<Func<T, TKey>> key, Expression<Func<TRelaction, TKey>> relectionKey)
        {
            Context = context;
            Key = key.Compile();
            RelectionKey = relectionKey.Compile();
            RelectionMember = relectionKey.GetMember();
        }
        public XContext Context { get; }
        public Func<T, TKey> Key { get; }
        public Func<TRelaction, TKey> RelectionKey { get; }
        public MemberInfo RelectionMember { get; }

        public abstract List<T> Invoke(List<T> list);
        public abstract T Invoke(T item);

        protected virtual Expression<Func<TRelaction, bool>> GetWhere(List<TKey> keys)
        {
            var p = Expression.Parameter(typeof(TRelaction), "r");
            var m = Expression.MakeMemberAccess(p, RelectionMember);

            var mthd = keys.GetType().GetMethod("Contains");
            var lt = Expression.Constant(keys);
            var keyType = typeof(TKey);
            if (m.Type == keyType)
            {
                var call = Expression.Call(lt, mthd, m);
                var lbd = Expression.Lambda<Func<TRelaction, bool>>(call, p);
                return lbd;
            }
            else if (m.Type.IsNullable())
            {
                var hasValue = Expression.Property(m, "HasValue");
                var value = Expression.Property(m, "Value");
                var call = Expression.Call(lt, mthd, value);
                var and = Expression.AndAlso(hasValue, call);
                var lbd = Expression.Lambda<Func<TRelaction, bool>>(and, p);
                return lbd;
            }
            else if (keyType.IsNullable())
            {
                var nullable = Expression.Convert(m, keyType);
                var call = Expression.Call(lt, mthd, nullable);
                var lbd = Expression.Lambda<Func<TRelaction, bool>>(call, p);
                return lbd;
            }
            throw Error.NotSupportedException("不支持的类型");
        }
        protected virtual Expression<Func<TRelaction, bool>> GetWhere(TKey key)
        {
            var p = Expression.Parameter(typeof(TRelaction), "r");
            var m = Expression.MakeMemberAccess(p, RelectionMember);
            var lt = Expression.Constant(key);
            var keyType = typeof(TKey);
            if (m.Type == keyType)
            {
                var call = Expression.Equal(lt, m);
                var lbd = Expression.Lambda<Func<TRelaction, bool>>(call, p);
                return lbd;
            }
            else if (m.Type.IsNullable())
            {
                var hasValue = Expression.Property(m, "HasValue");
                var value = Expression.Property(m, "Value");
                var call = Expression.Equal(lt, value);
                var and = Expression.AndAlso(hasValue, call);
                var lbd = Expression.Lambda<Func<TRelaction, bool>>(and, p);
                return lbd;
            }
            else if (keyType.IsNullable())
            {
                var nullable = Expression.Convert(m, keyType);
                var call = Expression.Equal(lt, nullable);
                var lbd = Expression.Lambda<Func<TRelaction, bool>>(call, p);
                return lbd;
            }
            throw Error.NotSupportedException("不支持的类型");
        }
    }
}