using System;
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
        public static void Each<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int num = 0;
            foreach (T current in source)
            {
                action(current, num++);
            }
        }
        public static void Each<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T current in source)
            {
                action(current);
            }
        }
        public static IEnumerable<S> Each<T, S>(this IEnumerable<T> source, Func<T, S> action)
        {
            foreach (T current in source)
            {
                yield return action(current);
            }
        }
    }
}