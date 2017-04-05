using System;
using System.Collections.Generic;

namespace XData.Common
{
    internal static class CommonMapper
    {
        public static object FromString(Type enumType, string value)
        {
            Dictionary<string, object> map = _types.Get(enumType, () =>
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

        static Cache<Type, Dictionary<string, object>> _types = new Cache<Type, Dictionary<string, object>>();
    }
}