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

        /// <summary>
        /// 从一个序列中创建树
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源序列</param>
        /// <param name="isChildOf">判定一个节点是另一个节点的子节点的函数</param>
        /// <param name="childrenSetter">将子节点集合设置为父节点的子节点的函数</param>
        /// <param name="sorter">同一父节点的兄弟节点的排序函数</param>
        /// <returns></returns>
        public static List<T> CreateTree<T>(this List<T> source, Func<T, T, bool> isChildOf, Action<T, List<T>> childrenSetter, Func<List<T>, List<T>> sorter = null)
        {
            sorter = sorter ?? (x => x);
            var roots = source.Where(x => source.All(y => Equals(y, x) || !isChildOf(y, x))).ToList();
            var noRoots = source.Except(roots).ToList();

            void SetChildren(T node)
            {
                T root = node;
                List<T> children = noRoots.Where(x => isChildOf(x, root)).ToList();

                children = sorter(children);
                childrenSetter(root, children);
                children.ForEach(x => noRoots.Remove(x));
                children.ForEach(SetChildren);
            }

            roots = sorter(roots);
            roots.ForEach(SetChildren);

            return roots;
        }
    }
}