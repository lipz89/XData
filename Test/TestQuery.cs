using System;
using System.Collections.Generic;
using Newtonsoft.Json;

using NUnit.Framework;
using Test.Models;
using XData.Common;
using XData.Core;


namespace Test
{
    public class TestQuery : BaseTest
    {
        [Test]
        public void InitValues()
        {
            var someValues = new List<SomeValues>();
            var rd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < 100; i++)
            {
                var sv = new SomeValues()
                {
                    Id = i,
                    ValueBit = false,
                    ValueInt = i,
                    ValueInt2 = rd.Next(Int32.MaxValue),
                    ValueNVarchar = "测试",
                    ValueVarchar = i + "abc" + i + "d" + i + "文本",
                    ValueBigInt = i,
                    ValueChar = 'A',
                    ValueDate = DateTime.Today,
                    ValueDatetime = DateTime.Now,
                    ValueDatetime2 = DateTime.Now,
                    ValueDatetimeOffset = DateTimeOffset.Now,
                    ValueDecimal = (decimal)rd.NextDouble(),
                    ValueFloat = (float)rd.NextDouble(),
                    ValueNumeric = (decimal)rd.NextDouble(),
                    ValueReal = rd.NextDouble(),
                    ValueText = "测试" + i + "文本",
                    ValueTinyint = (byte)i,
                    ValueNChar = '测',
                };
                someValues.Add(sv);
            }

            someValues[0].ValueVarchar = "testAProperty";
            someValues[1].ValueVarchar = "testBField";
            someValues[2].ValueVarchar = "testVarA";
            someValues[3].ValueVarchar = "testVarB";

            Context.DeleteBy<SomeValues>(null);
            var row = Context.Insert<SomeValues>(someValues);
            Assert.AreEqual(row, 100);
        }

        private string A { get; set; } = "testAProperty";
        private static string B = "testBField";
        [Test]
        public void Test()
        {
            var ss = new List<string> { "testAProperty", "testBField", "testVarA", "testVarB", "testVarC", "testAPropertyC" };
            var query = Context.Query<SomeValues>().Where(x => ss.Contains(x.ValueVarchar)).OrderBy(x => x.ValueVarchar).Top(10);

            var list = query.ToList();
            Console.WriteLine(list.Count);
        }
        [Test]
        public void Test2()
        {
            var a = "testVarA";
            var b = "testVarB";

            var query2 = Context.Query<SomeValues>().Where(x => x.ValueVarchar == A).FirstOrDefault();
            var query3 = Context.Query<SomeValues>().Where(x => x.ValueVarchar == B).FirstOrDefault();
            var query4 = Context.Query<SomeValues>().Where(x => x.ValueVarchar == a).FirstOrDefault();
            var query5 = Context.Query<SomeValues>().Where(x => x.ValueVarchar == b).FirstOrDefault();
            Console.WriteLine(JsonConvert.SerializeObject(query2));
            Console.WriteLine(JsonConvert.SerializeObject(query3));
            Console.WriteLine(JsonConvert.SerializeObject(query4));
            Console.WriteLine(JsonConvert.SerializeObject(query5));
        }

        [Test]
        public void TestLike()
        {
            string pattern = "%abc_d%";
            var query = Context.Query<SomeValues>().Where(x => x.ValueVarchar.SqlLike(pattern)).ToList();
            Console.WriteLine(query.Count);
        }

        [Test, Order(100)]
        public void TestAggregate()
        {
            Context.DeleteBy<SomeValues>(null);

            var query = Context.Query<SomeValues>();

            var count = query.Count();
            Assert.AreEqual(count, 0);

            Assert.Throws<XDataException>(() =>
            {
                var max = query.Max(x => x.ValueInt);
                Console.WriteLine(max);
            });
            Assert.Throws<XDataException>(() =>
            {
                var min = query.Min(x => x.Id);
                Console.WriteLine(min);
            });

            var sum = query.Sum(x => x.ValueInt);
            Assert.AreEqual(sum, 0);
            var avg = query.Avg(x => x.ValueInt);
            Assert.AreEqual(avg, 0);
        }
        [Test]
        public void TestAggregate2()
        {
            var query = Context.Query<SomeValues>();

            var count = query.Count();
            Assert.AreEqual(count, 100);

            var max = query.Max(x => x.ValueInt);
            Assert.AreEqual(max, 99);
            var min = query.Min(x => x.ValueInt);
            Assert.AreEqual(min, 0);
            var sum = query.Sum(x => x.ValueInt);
            Assert.AreEqual(sum, 4950);
            var avg = query.Avg(x => x.ValueInt);
            Assert.AreEqual(avg, 49);
        }

        [Test]
        public void TestQueryPage()
        {
            var query = Context.Query<SomeValues>();

            var page = query.ToPage(2, 10);
            Console.WriteLine(page);
            Console.WriteLine(JsonConvert.SerializeObject(page));
        }

        [Test]
        public void TestQueryIEnumerable()
        {
            var query = Context.Query<SomeValues>().Where(x => (x.ValueVarchar + x.ValueVarchar).Length < 20).Top(5);

            var enumer = query.ToList();
            Console.WriteLine(enumer.Count);
            Console.WriteLine(JsonConvert.SerializeObject(enumer));
            enumer.Clear();

            var ms = query.ToList();
            Console.WriteLine(ms.Count);
        }

        [Test]
        public void TestOrder()
        {
            var query = Context.Query<SomeValues>().Where(x => x.ValueInt.Between(10, 15)).OrderBy(x => x.ValueInt + 1);
            var l = query.ToList();
            Console.WriteLine(l.Count);
        }

        [Test]
        public void TestSingle()
        {
            var q = Context.GetFirstOrDefault<SomeValues>(x => x.ValueInt == 99);
            Console.WriteLine(JsonConvert.SerializeObject(q));


            var q2 = Context.GetByKey<SomeValues>(99);
            Console.WriteLine(JsonConvert.SerializeObject(q2));

            var q3 = Context.GetByKey<SomeValues>(40);
            Console.WriteLine(JsonConvert.SerializeObject(q3));
        }
    }
}