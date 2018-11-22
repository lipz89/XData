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
        /// ��һ�������д�����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">Դ����</param>
        /// <param name="isChildOf">�ж�һ���ڵ�����һ���ڵ���ӽڵ�ĺ���</param>
        /// <param name="childrenSetter">���ӽڵ㼯������Ϊ���ڵ���ӽڵ�ĺ���</param>
        /// <param name="sorter">ͬһ���ڵ���ֵܽڵ��������</param>
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