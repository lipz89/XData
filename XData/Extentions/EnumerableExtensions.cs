using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XData.Extentions
{
    internal static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty(this IEnumerable source)
        {
            return source == null || !source.Cast<object>().Any();
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
    }
}