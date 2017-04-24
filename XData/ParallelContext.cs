using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using XData.Common;
using XData.Extentions;

namespace XData
{
    /// <summary>
    /// 数据库并行上下文
    /// </summary>
    public class ParallelContext
    {
        private static readonly Cache<string, WorkPool<XContext>> cache = new Cache<string, WorkPool<XContext>>();
        private readonly WorkPool<XContext> dbBag;
        private static readonly int MaxCount = Environment.ProcessorCount + 10;

        internal ParallelContext(string connectionString, string providerName)
        {
            dbBag = cache.Get(connectionString + providerName,
                              () => new WorkPool<XContext>(() => new XContext(connectionString, providerName), MaxCount));
        }

        #region 并行

        /// <summary>
        /// 对序列中的元素并行执行数据库操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public void ForEach<T>(IEnumerable<T> source, Action<XContext, T> action)
        {
            if (!source.IsNullOrEmpty())
            {
                Parallel.ForEach(source, item =>
                                 {
                                     dbBag.Do(db => { action(db, item); });
                                 });
            }
        }

        /// <summary>
        /// 对序列中的元素并行执行数据库操作，并返回包含每个元素处理结果的序列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public List<TResult> ForEach<T, TResult>(IEnumerable<T> source, Func<XContext, T, TResult> action)
        {
            var rsts = new List<TResult>();
            if (!source.IsNullOrEmpty())
            {
                Parallel.ForEach(source, () => default(TResult), (item, state, r) =>
                                 {
                                     var rst = dbBag.Do(db => action(db, item));
                                     rsts.Add(rst);
                                     return rst;
                                 }, singleResult => { });
            }

            return rsts;
        }

        #endregion
    }
}