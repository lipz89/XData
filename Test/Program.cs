using System.Configuration;
using XData;

namespace Test
{
    class Program
    {
        static Program()
        {
            var connCfg = ConfigurationManager.ConnectionStrings["test"];

            if (connCfg != null)
            {
                SqlConnectionString = connCfg.ConnectionString;
                SqlProvider = connCfg.ProviderName;
            }
        }

        public static string SqlConnectionString;

        public static string SqlProvider;

        public static XContext NewContext()
        {
            return new XContext(SqlConnectionString, SqlProvider);
        }
    }
}