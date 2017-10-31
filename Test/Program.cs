using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

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
        static void Main(string[] args)
        {
            new TestEnumer().Test(5, 10);

            Console.Read();
        }

        public static string SqlConnectionString;

        public static string SqlProvider;

        public static XContext NewContext()
        {
            return new XContext(Program.SqlConnectionString, Program.SqlProvider);
        }
    }


    public class TestEnumer
    {
        private readonly int count;

        public TestEnumer(int count = 100)
        {
            this.count = count;
        }
        public IEnumerable<int> Get()
        {
            for (int i = 0; i < count; i++)
            {
                yield return GetResult(i);
            }
        }

        public void Test(int skip, int take)
        {
            var enumer = Get();
            var e20 = enumer.Skip(skip).Take(take);

            var lst = new List<int>();
            foreach (var i in e20)
            {
                lst.Add(i);
            }

            foreach (var i in lst)
            {
                Console.Write(i + "   ");
            }
        }

        private int GetResult(int i)
        {
            Console.WriteLine(i + " _ ");
            return i + 1;
        }
    }

    public class TestMember
    {

        public void Test()
        {

        }

        class BT
        {
            public int Int { get; set; }
        }
        class T1 : BT
        {

        }

        class T2 : BT
        {

        }
    }
}
