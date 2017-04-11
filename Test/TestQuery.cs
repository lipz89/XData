﻿using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using NUnit.Framework;

using Winning.SPD.SCM.Domain;

using XData;
using XData.Common;
using XData.Core;
using XData.Meta;


namespace Test
{
    public class TestQuery
    {
        public TestQuery()
        {
            MapperConfig.HasTableName<TestModel>("Test");
            MapperConfig.HasColumnName<TestModel>(x => x.Text, "Name");
            MapperConfig.IgnoreColumn<Supplier>(x => x.RowVersion);
            MapperConfig.IgnoreColumn<Menu>(x => x.RowVersion);
        }

        //private string a { get; set; } = "鄂20160196";
        private static string b = "粤20160402";
        [Test]
        public void Test()
        {
            var a = "鄂20160196";
            //var b = "粤20160402";
            var ss = new List<string> { "鄂20160196", "粤20160402", "苏20160297" };
            var db = Program.NewContext();
            var query = db.Query<Supplier>().Where(x => ss.Contains(x.Code)).OrderBy(x => x.Name).Top(10);

            var sql = query.ToSql();
            //var pas = query.Parameters;
            ////var table = db.GetDataTable(sql, pas.ToArray());

            //var list = db.Query<Supplier>(sql, pas.ToArray()).ToList();

            var list = query.ToList();
            Console.WriteLine(list.Count);
            Console.WriteLine("--cache:");
            list = query.ToList();
            Console.WriteLine(list.Count);
        }

        [Test]
        public void TestLike()
        {
            string pattern = "__s%-[ds]%";
            var db = Program.NewContext();
            var query = db.Query<Menu>().Where(x => x.Code.SqlLike(pattern)).ToList();
            Console.WriteLine(query.Count);
        }

        [Test]
        public void TestAggregate()
        {
            var db = Program.NewContext();
            var query = db.Query<TestModel>();

            var count = query.Count();
            Console.WriteLine(count);

            var max = query.Max(x => x.Index);
            Console.WriteLine(max);
            var min = query.Min(x => x.ID);
            Console.WriteLine(min);
            var sum = query.Sum(x => x.Index);
            Console.WriteLine(sum);
            var avg = query.Avg(x => x.Index);
            Console.WriteLine(avg);

            Console.WriteLine("--cache:");
            count = query.Count();
            Console.WriteLine(count);

            max = query.Max(x => x.Index);
            Console.WriteLine(max);
            min = query.Min(x => x.ID);
            Console.WriteLine(min);
            sum = query.Sum(x => x.Index);
            Console.WriteLine(sum);
            avg = query.Avg(x => x.Index);
            Console.WriteLine(avg);
        }

        [Test]
        public void TestQueryPage()
        {
            var db = Program.NewContext();
            var query = db.Query<TestModel>();

            var page = query.ToPage(2, 10);
            Console.WriteLine(JsonConvert.SerializeObject(page));
            Console.WriteLine("--cache:");
            page = query.ToPage(2, 10);
            Console.WriteLine(JsonConvert.SerializeObject(page));
        }

        [Test]
        public void TestQueryIEnumerable()
        {
            var db = Program.NewContext();
            var query = db.Query<TestModel>().Where(x => (x.Text + x.Text).Length < 20).Top(5);

            var enumer = query.ToList();
            Console.WriteLine(JsonConvert.SerializeObject(enumer));
            enumer.Clear();

            var ms = query.ToList();
            Console.WriteLine(ms.Count);
        }

        [Test]
        public void TestOrder()
        {
            var db = Program.NewContext();
            var query = db.Query<TestModel>().Where(x => x.Index < 10).OrderBy(x => x.Index + 1);
            query.ToList();
        }

        [Test]
        public void TestSingle()
        {
            var db = Program.NewContext();
            var q = db.GetFirstOrDefault<TestModel>(x => x.Index == 100);
            Console.WriteLine(JsonConvert.SerializeObject(q));


            var q2 = db.GetByKey<TestModel>(100, x => x.ID);
            Console.WriteLine(JsonConvert.SerializeObject(q2));

            var q3 = db.GetByKey<TestModel>(40);
            Console.WriteLine(JsonConvert.SerializeObject(q3));
        }
    }
}