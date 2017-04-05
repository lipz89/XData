using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Winning.SPD.SCM.Domain;

using XData.Extentions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = typeof(Dictionary<string, object>);
            var ps = t.GetProperties();
            var index = t.GetProperty("item");

            var m = t.GetMethods();

            //new TestDelete().Test();
            //new TestInsert().Test();

            Console.Read();
        }

        public const string SqlConnectionString = "data source=.;initial catalog=DMSP;user id=sa;password=111111;MultipleActiveResultSets=True";

        public const string EntityString = @"Provider=System.Data.SqlClient;Metadata=res://*/;Provider Connection String= 'Data Source=.;Initial Catalog = DMSP;user id=sa;password=111111; Integrated Security = True; Connection Timeout = 60'";
        public const string SqlProvider = "System.Data.SqlClient";
    }

    class TestModel
    {
        public TestModel(int id,int v)
        {
            ID = id;
        }
        public int ID { get; set; }
        public int Index { get; set; }
        public string Text { get; set; }
    }

    class SubMenu
    {
        public SubMenu(string name)
        {
            this.Name = name;
        }
        public string Action { get; set; }
        public string Code { get; set; }
        public string Controller { get; set; }
        public string Name { get; set; }
    }
}
