using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using Winning.SPD.SCM.Domain;

using XData.Meta;

namespace Test
{
    public class TestParallel
    {
        public TestParallel()
        {
            MapperConfig.HasKey<Menu>(x => x.ID);
            MapperConfig.IgnoreColumn<Menu>(x => x.RowVersion);
        }
        [Test]
        public void Test()
        {
            var dbp = Program.NewContext().AsParallel();
            var list = new List<Menu>();
            for (int i = 0; i < 20; i++)
            {
                list.Add(new Menu()
                         {
                             ID = Guid.NewGuid(),
                             Name = "测试" + i,
                             Code = "Test" + i,
                             MenuLevel = i,
                             IndexID = i,
                         });
            }

            Console.WriteLine("start:" + DateTime.Now.ToString("HH:mm:ss fff"));
            dbp.ForEach(list, (db, item) =>
                        {
                            Thread.Sleep(1000);
                            item.CreateTime = DateTime.Now;
                            db.Insert(item);
                        });
            Console.WriteLine("wait:" + DateTime.Now.ToString("HH:mm:ss fff"));
        }
        [Test]
        public void Test2()
        {
            var dbp = Program.NewContext().AsParallel();
            var list = new List<Menu>();
            for (int i = 0; i < 20; i++)
            {
                list.Add(new Menu()
                         {
                             ID = Guid.NewGuid(),
                             Name = "测试" + i,
                             Code = "Test" + i,
                             MenuLevel = i,
                             IndexID = i,
                         });
            }

            Console.WriteLine("start:" + DateTime.Now.ToString("HH:mm:ss fff"));
            var rsts = dbp.ForEach(list, (db, item) =>
                                   {
                                       Thread.Sleep(1000);
                                       item.CreateTime = DateTime.Now;
                                       return db.Insert(item);
                                   });

            Console.WriteLine("all:" + rsts.Count);
            Console.WriteLine("true:" + rsts.Count(x => x));
            Console.WriteLine("wait:" + DateTime.Now.ToString("HH:mm:ss fff"));
        }
    }
}