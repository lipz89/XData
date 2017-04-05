using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using XData.Common;
using XData.Extentions;
using XData.Meta;

namespace XData.Core
{
    internal class Reader<T> : IReader<T>
    {
        private readonly TableMeta tableMeta;
        private Func<object[], T> anonymousMapper;
        private Func<Dictionary<string, object>, T> objectMapper;

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
                var mapper = GetMapper(type, srcType);
                var val = mapper(value);
                return (T)val;
            }

            if (type.IsAnonymousType())
            {
                return ReadAnonymous(reader);
            }
            return ReadObject(reader);
        }

        private T ReadAnonymous(IDataReader reader)
        {
            var cnt = reader.FieldCount;
            var constructor = tableMeta.Type.GetConstructors().FirstOrDefault();
            var pars = constructor.GetParameters();
            var objs = new object[cnt];
            for (int i = 0; i < pars.Length; i++)
            {
                var val = IfDbNull(reader[pars[i].Name]);
                objs[i] = val;
            }
            var mapper = GetAnonymousMapper(constructor, pars);
            return mapper(objs);
        }
        private Func<object[], T> GetAnonymousMapper(ConstructorInfo constructor, ParameterInfo[] parameter)
        {
            if (anonymousMapper == null)
            {
                var param = Expression.Parameter(typeof(object[]));
                var ars = new Expression[parameter.Length];
                for (int i = 0; i < parameter.Length; i++)
                {
                    var t = parameter[i].ParameterType;
                    ars[i] = Expression.Convert(Expression.ArrayIndex(param, Expression.Constant(i)), t);
                }
                var obj = Expression.New(constructor, ars);
                var lambda = Expression.Lambda<Func<object[], T>>(obj, param);
                anonymousMapper = lambda.Compile();
            }
            return anonymousMapper;
        }

        private T ReadObject(IDataReader reader)
        {
            //var t = Activator.CreateInstance<T>();
            var dic = new Dictionary<string, object>();
            var cnt = reader.FieldCount;
            for (int i = 0; i < cnt; i++)
            {
                var name = reader.GetName(i);
                var column = tableMeta.Columns.FirstOrDefault(x => x.ColumnName == name);
                if (column != null)
                {
                    var dstType = column.Type;
                    var member = column.Member;
                    if (!reader.IsDBNull(i))
                    {
                        var srcType = reader.GetFieldType(i);
                        var value = IfDbNull(reader.GetValue(i));
                        if (dstType.IsAssignableFrom(srcType))
                        {
                            //column.SetValue(t, value);
                            dic.Add(column.ColumnName, value);
                        }
                        else
                        {
                            var mapper = GetMapper(dstType, srcType);
                            var val = mapper(value);
                            //column.SetValue(t, val);
                            dic.Add(column.ColumnName, val);
                        }
                    }
                    else if (!dstType.IsNullable() && !dstType.IsClass && !dstType.IsInterface)
                    {
                        //dic.Add(column.ColumnName, null);
                        throw Error.Exception("成员 " + member.Name + " 不能设置为空值。");
                    }
                }
            }
            //return t;
            var omapper = GetObjectMapper();
            return omapper(dic);
        }

        private Func<Dictionary<string, object>, T> GetObjectMapper()
        {
            if (objectMapper == null)
            {
                var constructor = tableMeta.Type.GetDefaultCtor()
                    ?? tableMeta.Type.GetConstructors().FirstOrDefault();
                if (constructor == null)
                {
                    throw Error.Exception("类型" + tableMeta.Type + "没有公开的构造函数。");
                }
                var param = Expression.Parameter(typeof(Dictionary<string, object>));
                var columns = tableMeta.Columns.ToList();
                var parameter = constructor.GetParameters();
                var newArgs = new Expression[parameter.Length];
                for (int i = 0; i < parameter.Length; i++)
                {
                    var t = parameter[i].ParameterType;
                    var name = parameter[i].Name;
                    var column = columns.FirstOrDefault(x => x.Name.IsSameField(name));
                    Expression val;
                    if (column != null)
                    {
                        var key = Expression.Constant(column.ColumnName);
                        var ife = Expression.Call(param, Constans.DictionaryContainsKey, key);
                        var value = Expression.MakeIndex(param, Constans.DictionaryIndex, new[] { key });
                        val = Expression.Condition(ife, Expression.Convert(value, t), Expression.Default(t));
                        columns.Remove(column);
                    }
                    else
                    {
                        val = Expression.Default(t);
                    }
                    newArgs[i] = val;
                }
                var obj = Expression.New(constructor, newArgs);
                var binds = new List<MemberAssignment>();
                foreach (var column in columns.Where(x => x.CanWrite()))
                {
                    var key = Expression.Constant(column.ColumnName);
                    var ife = Expression.Call(param, Constans.DictionaryContainsKey, key);
                    var value = Expression.MakeIndex(param, Constans.DictionaryIndex, new[] { key });
                    var condition = Expression.Condition(ife, Expression.Convert(value, column.Type), Expression.Default(column.Type));
                    var bind = Expression.Bind(column.Member, condition);
                    binds.Add(bind);
                }
                var init = Expression.MemberInit(obj, binds);
                var lambda = Expression.Lambda<Func<Dictionary<string, object>, T>>(init, param);
                objectMapper = lambda.Compile();
            }
            return objectMapper;
        }

        private Func<object, object> GetMapper(Type dstType, Type srcType)
        {
            if (dstType.IsNullable())
            {
                return GetMapper(dstType.NonNullableType(), srcType);
            }
            if (dstType.IsEnum && srcType.IsIntegralType())
            {
                return src => CommonMapper.FromIntegral(dstType, src);
            }
            if (dstType.IsEnum && srcType == typeof(string))
            {
                return src => CommonMapper.FromString(dstType, (string)src);
            }
            if (dstType == typeof(Guid) && srcType == typeof(byte[]))
            {
                return src => new Guid((byte[])src);
            }
            if (dstType == typeof(bool))
            {
                return src =>
                {
                    var s = src.ToString();
                    return s != "0" && !s.Equals("false", StringComparison.CurrentCultureIgnoreCase);
                };
            }
            return src =>
            {
                var cvt = TypeDescriptor.GetConverter(dstType);
                if (cvt.CanConvertFrom(srcType))
                {
                    return cvt.ConvertFrom(src);
                }
                cvt = TypeDescriptor.GetConverter(src);
                if (cvt.CanConvertTo(dstType))
                {
                    return cvt.ConvertTo(src, dstType);
                }

                return Convert.ChangeType(src, dstType, null);
            };
        }

        private object IfDbNull(object obj)
        {
            if (obj == DBNull.Value)
                return null;
            return obj;
        }
    }
}
