using System;
using System.Collections.Generic;
using System.Data;

using XData.Common;
using XData.Meta;

namespace XData.Core
{
    /// <summary>
    /// 数据读取器
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
    }
}
