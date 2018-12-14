using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        [Test]
        public void TestLike2()
        {
            var list = new List<SomeValues>()
            {
                new SomeValues {ValueVarchar = "aaaabcddddd"}
            };
            string pattern = "%abc_d%";
            var query = list.Where(x => x.ValueVarchar.SqlLike(pattern)).ToList();
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


        [Test]
        public void TestLambda()
        {
            //委托：方法作为参数传递
            var r1 = Result(3, 4, Sum);
            //使用匿名方法传递委托
            var r4 = Result(3, 4, delegate (int x, int y) { return x + y; });
            //语句lambda 传递委托
            var r2 = Result(3, 4, (a, b) => { return a - b; });
            //表达式lambda 传递委托
            var r3 = Result(3, 4, (a, b) => a * b);
            Console.ReadLine();
        }
        private static int Result(int a, int b, Func<int, int, int> @delegate)
        {
            return @delegate(a, b);
        }
        private static int Sum(int a, int b)
        {
            return a + b;
        }


        [Test]
        public void TestExpression()
        {
            Func<int, int> func = x => x + 1;               //Code 
            Expression<Func<int, int>> exp = x => x + 1;    //Data

            //创建表达式树：Expression<Func<int, int>> exp = x => x + 1;
            ParameterExpression param = Expression.Parameter(typeof(int), "x");
            ConstantExpression value = Expression.Constant(1, typeof(int));
            BinaryExpression body = Expression.Add(param, value);
            Expression<Func<int, int>> lambdatree = Expression.Lambda<Func<int, int>>(body, param);

            Func<int, int> func2 = exp.Compile();
            Func<int, int> func3 = lambdatree.Compile();
        }

        [Test]
        public void TestExpression2()
        {
            Expression<Func<int, int>> exp = x => x + 1;    //Data
        }

        [Test]
        public void TestExpression3()
        {
            ParameterExpression param = Expression.Parameter(typeof(int), "x");
            Expression<Func<int, int>> exp = Expression.Lambda<Func<int, int>>(Expression.Add(param, Expression.Constant(1, typeof(int))), param);
        }
    }

    public class Test
    {
        public void TestFunc()
        {
            Func<int, int> func = x => x + 1;
        }
        public void TestExpression1()
        {
            Expression<Func<int, int>> exp = x => x + 1;
        }
        public void TestExpression2()
        {
            ParameterExpression param = Expression.Parameter(typeof(int), "x");
            Expression<Func<int, int>> exp = Expression.Lambda<Func<int, int>>(Expression.Add(param, Expression.Constant(1, typeof(int))), param);
        }
    }
}