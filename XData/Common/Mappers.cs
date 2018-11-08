using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using XData.Common.Fast;
using XData.Extentions;
using XData.Meta;

namespace XData.Common
{
    internal static class Mappers
    {
        private static readonly Cache<Type, Dictionary<string, object>> types = new Cache<Type, Dictionary<string, object>>();
        private static readonly Cache<Type, Func<Dictionary<string, object>, object>> objectMappers = new Cache<Type, Func<Dictionary<string, object>, object>>();


        public static object FromString(Type enumType, string value)
        {
            Dictionary<string, object> map = types.Get(enumType, () =>
            {
                var values = Enum.GetValues(enumType);
                var newmap = new Dictionary<string, object>(values.Length, StringComparer.InvariantCultureIgnoreCase);
                foreach (var v in values)
                {
                    newmap.Add(v.ToString(), v);
                }
                return newmap;
            });

            return map[value];
        }

        public static object FromIntegral(Type enumType, object value)
        {
            var udType = Enum.GetUnderlyingType(enumType);
            var val = value;
            if (value.GetType() != udType)
            {
                val = Convert.ChangeType(value, udType, null);
            }
            return Enum.ToObject(enumType, val);
        }

        public static Func<object, object> GetMapper(Type dstType, Type srcType)
        {
            if (dstType.IsNullable())
            {
                return GetMapper(dstType.NonNullableType(), srcType);
            }
            if (dstType.IsEnum && srcType.IsIntegralType())
            {
                return src => FromIntegral(dstType, src);
            }
            if (dstType.IsEnum && srcType == typeof(string))
            {
                return src => FromString(dstType, (string)src);
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
                        var val = t.GetDefaultValue();
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
                        else if (!DbTypes.IsSimpleType(t))
                        {
                            var vs = param.Where(x => x.Key.StartsWith(n + "-"))
                                               .ToDictionary(x => x.Key.Substring(n.Length + 1), x => x.Value);
                            var pMeta = TableMeta.From(t);
                            var mapper = GetObjectMapper(pMeta);
                            val = mapper(vs);
                            foreach (var vsKey in vs.Keys)
                            {
                                param.Remove(n + "-" + vsKey);
                            }
                        }
                        args.Add(val);
                    }

                    var instance = constructor.FastCreate(args.ToArray());

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
                                prop.FastSetValue(instance, propValue);
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
    }
}