using System;

using Newtonsoft.Json;

using NUnit.Framework;

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
            var db = Program.NewContext();
            var qu = db.Query<Menu>().Where(x => x.Action != null && !x.IsDeleted);
            var sql = qu.ToSql();
            //Console.WriteLine(sql);
            var q1 = qu.Select(x => new { x.Name, x.Code }).Where(x => x.Code.Length > 1);
            var sql1 = q1.ToSql();
            Console.WriteLine(sql1);
            Assert.Throws<NotSupportedException>(() =>
            {
                var q2 = q1.Select(x => new { Test = x.Name + x.Code, }).OrderBy(x => x.Test);
                var sql2 = q2.ToSql();
                Console.WriteLine(sql2);
            });
        }

        [Test]
        public void Test6()
        {
            var db = Program.NewContext();
            var qu = db.Query<Menu>().Where(x => x.Action != null );
            var q1 = qu.Select(x => new { x.Name, x.Code });
            var q22 = q1.Select(x => x.Name + x.Code).OrderBy(x => x);
            var sql22 = q22.ToSql();
            var result = q22.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test5()
        {
            var db = Program.NewContext();
            var q3 = db.Query<Menu>().Select(x => x.Controller).Where(x => x.Contains("o"));
            var sql3 = q3.ToSql();

            var result = q3.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test2()
        {
            var db = Program.NewContext();
            var qu = db.Query<Menu>().Where(x => x.Action != null);
            var sql = qu.ToSql();
            //Console.WriteLine(sql);

            var q2 = qu.Select(x => new { Name = x.Controller, Code = x.Code }).Where(x => x.Code.Length > 1);
            var sql2 = q2.ToSql();
            //Console.WriteLine(sql2);

            var result = q2.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test3()
        {
            var db = Program.NewContext();
            var qu = db.Query<Menu>();
            var sql = qu.ToSql();
            //Console.WriteLine(sql);

            var q2 = qu.Select(x => new { N = x.Name, x.Code, Inner = new { x.Controller, x.Action } }).Where(x => x.Code.Length > 2);
            var sql2 = q2.ToSql();
            //Console.WriteLine(sql2);

            var result = q2.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test4()
        {
            var db = Program.NewContext();
            var qu = db.Query<Menu>().Where(x => x.Action != null);
            var sql = qu.ToSql();
            //Console.WriteLine(sql);

            var q2 = qu.Select(x => new Menu { Name = x.Controller, Code = x.Code }).Where(x => x.Code.Length > 1);
            var sql2 = q2.ToSql();
            //Console.WriteLine(sql2);

            var result = q2.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}