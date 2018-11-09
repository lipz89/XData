using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Test.Models;
using XData.Meta;

namespace Test
{
    public class TestInclude : BaseTest
    {
        [Test, Order(0)]
        public void TestInsert()
        {
            var r1 = Context.DeleteBy<Parent>(x => new int[] { 5, 6 }.Contains(x.Id));
            var r2 = Context.DeleteBy<Child>(x => x.ParentId.HasValue && new int[] { 5, 6 }.Contains(x.ParentId.Value));

            var parents = new List<Parent>
            {
                new Parent() {Id = 5, Name = "测试1", Code = "Test1"},
                new Parent() {Id = 6, Name = "测试2", Code = "Test2"},
            };

            var children = new List<Child>
            {
                new Child() {ParentId = 5, Code = "T111", Name = "T111",Id = 101},
                new Child() {ParentId = 5, Code = "T222", Name = "T222",Id = 102},
                new Child() {ParentId = 6, Code = "T333", Name = "T333",Id = 103},
                new Child() {ParentId = 6, Code = "T444", Name = "T444",Id = 104},
            };

            var row1 = Context.Insert<Parent>(parents);
            var row2 = Context.Insert<Child>(children);

            Assert.AreEqual(row1, 2);
            Assert.AreEqual(row2, 4);
        }
        [Test, Order(1)]
        public void Test()
        {
            var parents = Context.Query<Parent>().ToList();

            foreach (var parent in parents)
            {
                Console.WriteLine(parent);
            }

            var children = Context.Query<Child>().ToList();
            foreach (var child in children)
            {
                Console.WriteLine(child);
            }
        }

        [Test, Order(2)]
        public void TestIncludeChildren()
        {
            var query = Context.Query<Parent>()
                .Include(x => x.Children,
                x => x.Id,
                x => x.ParentId,
                (p, c) =>
                {
                    p.Children = c.ToList();
                    foreach (var child in c)
                    {
                        child.Parent = p;
                    }
                });

            var parents = query.ToList();

            foreach (var parent in parents)
            {
                Console.WriteLine(parent);
            }

            var pm = query.FirstOrDefault();
            Console.WriteLine(pm);
        }
        [Test, Order(3)]
        public void TestIncludeChildrenWithoutKey()
        {
            MapperConfig.HasKey<Parent>(x => x.Id);

            var query = Context.Query<Parent>()
                .Include(x => x.Children,
                x => x.ParentId,
                (p, c) =>
                {
                    p.Children = c.ToList();
                    foreach (var child in c)
                    {
                        child.Parent = p;
                    }
                });

            var parents = query.ToList();

            foreach (var parent in parents)
            {
                Console.WriteLine(parent);
            }

            var pm = query.FirstOrDefault();
            Console.WriteLine(pm);
        }

        [Test, Order(4)]
        public void TestIncludeParent()
        {
            var query = Context.Query<Child>()
                .Include(x => x.Parent,
                x => x.ParentId,
                x => x.Id,
                (c, p) =>
                         {
                             c.Parent = p;
                             if (p.Children == null)
                             {
                                 p.Children = new List<Child>();
                             }
                             p.Children.Add(c);
                         });

            var children = query.ToList();

            foreach (var child in children)
            {
                Console.WriteLine(child);
            }

            var pm = query.FirstOrDefault();
            Console.WriteLine(pm);
        }

        [Test, Order(5)]
        public void TestIncludeParentWithoutKey()
        {
            MapperConfig.HasKey<Parent>(x => x.Id);

            var query = Context.Query<Child>()
                .Include(x => x.Parent,
                x => x.ParentId,
                (c, p) =>
                {
                    c.Parent = p;
                    if (p.Children == null)
                    {
                        p.Children = new List<Child>();
                    }
                    p.Children.Add(c);
                });

            var children = query.ToList();

            foreach (var child in children)
            {
                Console.WriteLine(child);
            }

            var pm = query.FirstOrDefault();
            Console.WriteLine(pm);
        }
    }
}