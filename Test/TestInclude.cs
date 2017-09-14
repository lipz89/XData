using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using XData;
using XData.Meta;

namespace Test
{
    public class TestInclude
    {
        [Test, Order(1)]
        public void Test()
        {
            var db = Program.NewContext();

            var parents = db.Query<Parent>().ToList();

            foreach (var parent in parents)
            {
                Console.WriteLine(parent);
            }

            var children = db.Query<Child>().ToList();
            foreach (var child in children)
            {
                Console.WriteLine(child);
            }
        }

        [Test, Order(2)]
        public void TestIncludeChildren()
        {
            var db = Program.NewContext();

            var query = db.Query<Parent>()
                .Include(x => x.Children,
                x => x.ID,
                x => x.ParentID,
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
            var db = Program.NewContext();
            MapperConfig.HasKey<Parent>(x => x.ID);

            var query = db.Query<Parent>()
                .Include(x => x.Children,
                x => x.ParentID,
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
            var db = Program.NewContext();

            var query = db.Query<Child>()
                .Include(x => x.Parent,
                x => x.ParentID,
                x => x.ID,
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
            var db = Program.NewContext();
            MapperConfig.HasKey<Parent>(x => x.ID);

            var query = db.Query<Child>()
                .Include(x => x.Parent,
                x => x.ParentID,
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