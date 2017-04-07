using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

using XData.Common;
using XData.Extentions;
using XData.Meta;

namespace XData.Core
{
    internal static class Mappers
    {
        private static Cache<Type, Func<Dictionary<string, object>, object>> objectMappers = new Cache<Type, Func<Dictionary<string, object>, object>>();

        private static object GetDefaultValue(Type type)
        {
            var exp = Expression.Default(type);
            var lambda = Expression.Lambda(exp);
            return lambda.Compile().DynamicInvoke();
        }
        public static Func<Dictionary<string, object>, object> GetObjectMapper(TableMeta tableMeta)
        {
            return objectMappers.Get(tableMeta.Type, () =>
                                     {
                                         var constructor = tableMeta.Type.GetDefaultCtor()
                                                           ?? tableMeta.Type.GetConstructors().FirstOrDefault();
                                         if (constructor == null)
                                         {
                                             throw Error.Exception("类型" + tableMeta.Type + "没有公开的构造函数。");
                                         }
                                         var parameter = constructor.GetParameters();

                                         Func<Dictionary<string, object>, object> map = (param) =>
                                         {
                                             var columns = tableMeta.Columns.ToList();
                                             var args = new List<object>();
                                             foreach (var info in parameter)
                                             {
                                                 var t = info.ParameterType;
                                                 var n = info.Name;
                                                 var col = columns.FirstOrDefault(x => x.Name.IsSameField(n));
                                                 var val = GetDefaultValue(t);
                                                 if (col != null)
                                                 {
                                                     var key = col.ColumnName;
                                                     if (param.ContainsKey(key))
                                                     {
                                                         val = param[key];
                                                         if (val != null && !t.IsInstanceOfType(val))
                                                         {
                                                             var mapper = GetMapper(t, val.GetType());
                                                             val = mapper(val);
                                                         }
                                                         param.Remove(key);
                                                     }
                                                     columns.Remove(col);
                                                 }
                                                 else
                                                 {
                                                     var prop = tableMeta.Type.GetProperty(n);
                                                     if (!DbTypes.IsSimpleType(prop.PropertyType))
                                                     {
                                                         var vs = param.Where(x => x.Key.StartsWith(n + "-"))
                                                                       .ToDictionary(x => x.Key.Substring(n.Length + 1), x => x.Value);
                                                         var propMeta = TableMeta.From(prop.PropertyType);
                                                         var mapper = GetObjectMapper(propMeta);
                                                         val = mapper(vs);
                                                         foreach (var vsKey in vs.Keys)
                                                         {
                                                             param.Remove(n + "-" + vsKey);
                                                         }
                                                     }
                                                 }
                                                 args.Add(val);
                                             }
                                             var instance = constructor.Invoke(args.ToArray());

                                             foreach (var col in columns.Where(x => x.CanWrite()))
                                             {
                                                 var key = col.ColumnName;
                                                 if (param.ContainsKey(key))
                                                 {
                                                     var val = param[key];
                                                     if (val != null && !col.Type.IsInstanceOfType(val))
                                                     {
                                                         var mapper = GetMapper(col.Type, val.GetType());
                                                         val = mapper(val);
                                                     }
                                                     col.SetValue(instance, val);
                                                     param.Remove(key);
                                                 }
                                             }

                                             var keys = param.Where(x => x.Key.Contains("-")).ToList();
                                             if (keys.Any())
                                             {
                                                 var inners = keys.GroupBy(x => x.Key.Split('-')[0]);
                                                 foreach (var inner in inners)
                                                 {
                                                     var prop = tableMeta.Type.GetProperty(inner.Key);
                                                     if (prop != null && prop.CanWrite)
                                                     {
                                                         var vs = inner.ToDictionary(x => x.Key.Substring(inner.Key.Length + 1), x => x.Value);
                                                         var propMeta = TableMeta.From(prop.PropertyType);
                                                         var mapper = GetObjectMapper(propMeta);
                                                         var propValue = mapper(vs);
                                                         prop.SetValue(instance, propValue);
                                                     }
                                                     foreach (var result in inner.Select(x => x.Key))
                                                     {
                                                         param.Remove(result);
                                                     }
                                                 }
                                             }

                                             return instance;
                                         };

                                         return map;
                                     });
        }

        public static Func<object, object> GetMapper(Type dstType, Type srcType)
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
    }
}