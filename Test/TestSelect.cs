using System;

using Newtonsoft.Json;

using NUnit.Framework;
using Test.Models;

namespace Test
{
    public class TestSelect : BaseTest
    {

        [Test]
        public void Test()
        {
            var qu = Context.Query<Menu>().Where(x => x.Url != null);
            var sql = qu.ToSql();
            Console.WriteLine(sql);
            var q1 = qu.Select(x => new { x.Name, x.Code }).Where(x => x.Code.Length > 1);
            var sql1 = q1.ToSql();
            Console.WriteLine(sql1);

            var q2 = q1.Select(x => new { Test = x.Name + x.Code, }).OrderBy(x => x.Test);
            var sql2 = q2.ToSql();
            Console.WriteLine(sql2);
            var list = q2.ToList();
            Console.WriteLine(list.Count);

        }

        [Test]
        public void Test2()
        {
            var qu = Context.Query<Menu>().Where(x => x.Url != null);
            var sql = qu.ToSql();
            Console.WriteLine(sql);

            var q2 = qu.Select(x => new { Name = x.Url, x.Code }).Where(x => x.Code.Length > 1);
            var sql2 = q2.ToSql();
            Console.WriteLine(sql2);

            var result = q2.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test3()
        {
            var qu = Context.Query<Menu>();
            var sql = qu.ToSql();
            Console.WriteLine(sql);

            var q2 = qu.Select(x => new { N = x.Name, x.Code, Inner = new { x.Url, x.Code } }).Where(x => x.Code.Length > 2);
            var sql2 = q2.ToSql();
            Console.WriteLine(sql2);

            var result = q2.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test4()
        {
            var qu = Context.Query<Menu>().Where(x => x.Url != null);
            var sql = qu.ToSql();
            Console.WriteLine(sql);

            var q2 = qu.Select(x => new Menu { Name = x.Url, Code = x.Code }).Where(x => x.Code.Length > 1);
            var sql2 = q2.ToSql();
            Console.WriteLine(sql2);

            var result = q2.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test5()
        {
            var q3 = Context.Query<Menu>().Select(x => x.Url).Where(x => x.Contains("o"));
            var sql3 = q3.ToSql();
            Console.WriteLine(sql3);

            var result = q3.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        [Test]
        public void Test6()
        {
            var qu = Context.Query<Menu>().Where(x => x.Url != null);
            var q1 = qu.Select(x => new { x.Name, x.Code });
            var q22 = q1.Select(x => x.Name + x.Code).OrderBy(x => x);
            var sql22 = q22.ToSql();
            Console.WriteLine(sql22);
            var result = q22.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}