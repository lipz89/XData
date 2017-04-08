using System;
using System.Collections.Generic;
using System.Data;

using XData.Common;
using XData.Meta;

namespace XData.Core
{
    internal class Reader<T> : IReader<T>
    {
        private readonly TableMeta tableMeta;

        public Reader()
        {
            tableMeta = TableMeta.From<T>();
        }

        public T Read(IDataReader reader)
        {
            var type = typeof(T);
            if (DbTypes.ContainsType(type) && type != typeof(object))
            {
                var value = IfDbNull(reader.GetValue(0));
                var srcType = reader.GetFieldType(0);
                if (type.IsAssignableFrom(srcType))
                {
                    return (T)value;
                }
                var mapper = Mappers.GetMapper(type, srcType);
                var val = mapper(value);
                return (T)val;
            }

            return ReadObject(reader);
        }

        private T ReadObject(IDataReader reader)
        {
            var dic = new Dictionary<string, object>();
            var cnt = reader.FieldCount;
            for (int i = 0; i < cnt; i++)
            {
                var name = reader.GetName(i);
                var value = IfDbNull(reader.GetValue(i));
                dic.Add(name, value);
            }
            var omapper = Mappers.GetObjectMapper(tableMeta);
            return (T)omapper(dic);
        }
        private object IfDbNull(object obj)
        {
            return obj == DBNull.Value ? null : obj;
        }

        //private Func<Dictionary<string, object>, T> objectMapper;

        //private Func<Dictionary<string, object>, T> GetObjectMapper()
        //{
        //    if (objectMapper == null)
        //    {
        //        var constructor = tableMeta.Type.GetDefaultCtor()
        //            ?? tableMeta.Type.GetConstructors().FirstOrDefault();
        //        if (constructor == null)
        //        {
        //            throw Error.Exception("类型" + tableMeta.Type + "没有公开的构造函数。");
        //        }
        //        var columns = tableMeta.Columns.ToList();
        //        var parameter = constructor.GetParameters();
        //        var newArgs = new Expression[parameter.Length];
        //        var param = Expression.Parameter(typeof(Dictionary<string, object>));
        //        for (int i = 0; i < parameter.Length; i++)
        //        {
        //            var t = parameter[i].ParameterType;
        //            var name = parameter[i].Name;
        //            var column = columns.FirstOrDefault(x => x.Name.IsSameField(name));
        //            Expression val;
        //            if (column != null)
        //            {
        //                var key = Expression.Constant(column.ColumnName);
        //                var ife = Expression.Call(param, Constans.DictionaryContainsKey, key);
        //                var value = Expression.MakeIndex(param, Constans.DictionaryIndex, new[] { key });
        //                val = Expression.Condition(ife, Expression.Convert(value, t), Expression.Default(t));
        //                columns.Remove(column);
        //            }
        //            else
        //            {
        //                val = Expression.Default(t);
        //            }
        //            newArgs[i] = val;
        //        }
        //        var obj = Expression.New(constructor, newArgs);
        //        var binds = new List<MemberAssignment>();
        //        foreach (var column in columns.Where(x => x.CanWrite()))
        //        {
        //            var key = Expression.Constant(column.ColumnName);
        //            var ife = Expression.Call(param, Constans.DictionaryContainsKey, key);
        //            var value = Expression.MakeIndex(param, Constans.DictionaryIndex, new[] { key });
        //            var condition = Expression.Condition(ife, Expression.Convert(value, column.Type), Expression.Default(column.Type));
        //            var bind = Expression.Bind(column.Member, condition);
        //            binds.Add(bind);
        //        }
        //        var init = Expression.MemberInit(obj, binds);
        //        var lambda = Expression.Lambda<Func<Dictionary<string, object>, T>>(init, param);
        //        objectMapper = lambda.Compile();
        //    }
        //    return objectMapper;
        //}
    }
}
