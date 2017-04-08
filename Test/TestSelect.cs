using System;

using Newtonsoft.Json;

using NUnit.Framework;

using Winning.SPD.SCM.Domain;

using XData;
using XData.Meta;

namespace Test
{
    public class TestSelect
    {
        public TestSelect()
        {
            MapperConfig.IgnoreColumn<Menu>(x => x.RowVersion);
        }

        [Test]
        public void Test()
        {
            var db = new XContext(Program.SqlConnectionString, Program.SqlProvider);
            //var qu = db.Query<Menu>().Where(x => x.Action != null && !x.IsDeleted);
            //var sql = qu.ToSql();
            ////Console.WriteLine(sql);
            //var q1 = qu.Select(x => new { x.Name, x.Code }).Where(x => x.Code.Length > 7);
            //var sql1 = q1.ToSql();
            //var q2 = q1.Select(x => new { Test = x.Name + x.Code, }).OrderBy(x => x.Test);
            //var sql2 = q2.ToSql();
            //Console.WriteLine(sql2);

            var q3 = db.Query<Menu>().Select(x => x.Controller).Where(x => x.Contains("A"));
            var sql3 = q3.ToSql();

            var result = q3.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test2()
        {
            var db = new XContext(Program.SqlConnectionString, Program.SqlProvider);
            var qu = db.Query<Menu>().Where(x => x.Action != null);
            var sql = qu.ToSql();
            //Console.WriteLine(sql);

            var q2 = qu.Select(x => new  { Name = x.Controller, Code = x.Code }).Where(x => x.Code.Length > 1);
            var sql2 = q2.ToSql();
            //Console.WriteLine(sql2);

            var result = q2.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test3()
        {
            var db = new XContext(Program.SqlConnectionString, Program.SqlProvider);
            var qu = db.Query<Menu>();
            var sql = qu.ToSql();
            //Console.WriteLine(sql);

            var q2 = qu.Select(x => new { N = x.Name, x.Code, Inner = new { x.Controller, x.Action } }).Where(x => x.Code.Length > 7);
            var sql2 = q2.ToSql();
            //Console.WriteLine(sql2);

            var result = q2.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test4()
        {
            var db = new XContext(Program.SqlConnectionString, Program.SqlProvider);
            var qu = db.Query<Menu>().Where(x => x.Action != null);
            var sql = qu.ToSql();
            //Console.WriteLine(sql);

            var q2 = qu.Select(x => new Menu { Name = x.Controller, Code = x.Code }).Where(x => x.Code.Length > 18);
            var sql2 = q2.ToSql();
            //Console.WriteLine(sql2);

            var result = q2.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}