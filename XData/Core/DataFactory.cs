using System.Data;

namespace XData.Core
{
    /// <summary>
    /// 数据读取器工厂
    /// </summary>
    internal static class DataFactory
    {
        /// <summary>
        /// 用泛型类做缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static class Cache<T>
        {
            public static IReader<T> Instance { get; set; }
        }
        /// <summary>
        /// 获取一个数据读取器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IReader<T> Get<T>()
        {
            return Cache<T>.Instance ?? (Cache<T>.Instance = new Reader<T>());
        }
        /// <summary>
        /// 从数据库结果集流中读取一行数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T Read<T>(IDataReader reader)
        {
            return Get<T>().Read(reader);
        }
    }
}