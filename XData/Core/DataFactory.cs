using System.Data;

namespace XData.Core
{
    internal static class DataFactory
    {
        class Cache<T>
        {
            public static IReader<T> Instance { get; set; }
        }

        public static IReader<T> Get<T>()
        {
            return Cache<T>.Instance ?? (Cache<T>.Instance = new Reader<T>());
        }

        public static T Read<T>(IDataReader reader)
        {
            return Get<T>().Read(reader);
        }
    }
}