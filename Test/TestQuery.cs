using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using NUnit.Framework;

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
            //var query = db.Query<Supplier>().Where(x => ss.Contains(x.Code)).OrderBy(x => x.Name).Top(10);

            //var sql = query.ToSql();
            ////var pas = query.Parameters;
            //////var table = db.GetDataTable(sql, pas.ToArray());

            ////var list = db.Query<Supplier>(sql, pas.ToArray()).ToList();

            //var list = query.ToList();
            //Console.WriteLine(list.Count);
            //Console.WriteLine("--cache:");
            //list = query.ToList();
            //Console.WriteLine(list.Count);
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

            db.Delete<TestModel>(null);

            var query = db.Query<TestModel>();

            var count = query.Count();
            Assert.AreEqual(count, 0);

            Assert.Throws<XDataException>(() =>
            {
                var max = query.Max(x => x.Index);
                Console.WriteLine(max);
            });
            Assert.Throws<XDataException>(() =>
            {
                var min = query.Min(x => x.ID);
                Console.WriteLine(min);
            });

            var sum = query.Sum(x => x.Index);
            Assert.AreEqual(sum, 0);
            var avg = query.Avg(x => x.Index);
            Assert.AreEqual(avg, 0);

            Console.WriteLine("--cache:");
            count = query.Count();
            Console.WriteLine(count);

            Assert.Throws<XDataException>(() =>
            {
                var max = query.Max(x => x.Index);
                Console.WriteLine(max);
            });
            Assert.Throws<XDataException>(() =>
            {
                var min = query.Min(x => x.ID);
                Console.WriteLine(min);
            });

            sum = query.Sum(x => x.Index);
            Assert.AreEqual(sum, 0);
            avg = query.Avg(x => x.Index);
            Assert.AreEqual(avg, 0);
        }
        [Test]
        public void TestAggregate2()
        {
            var db = Program.NewContext();

            db.Delete<TestModel>(null);

            for (int i = 0; i < 10; i++)
            {
                db.Insert(new TestModel(i, i) { Index = i, Text = "Text" + i });
            }

            var query = db.Query<TestModel>();

            var count = query.Count();
            Assert.AreEqual(count, 10);

            var max = query.Max(x => x.Index);
            Assert.AreEqual(max, 9);
            var min = query.Min(x => x.ID);
            Assert.AreEqual(min, 0);
            var sum = query.Sum(x => x.Index);
            Assert.AreEqual(sum, 45);
            var avg = query.Avg(x => x.Index);
            Assert.AreEqual(avg, 4);

            Console.WriteLine("--cache:");
            count = query.Count();
            Assert.AreEqual(count, 10);

            max = query.Max(x => x.Index);
            Assert.AreEqual(max, 9);
            min = query.Min(x => x.ID);
            Assert.AreEqual(min, 0);
            sum = query.Sum(x => x.Index);
            Assert.AreEqual(sum, 45);
            avg = query.Avg(x => x.Index);
            Assert.AreEqual(avg, 4);
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
            var query = db.Query<TestModel>().Where(x => x.Index.Between(10, 15)).OrderBy(x => x.Index + 1);
            var l = query.ToList();
            Console.WriteLine(l.Count);
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