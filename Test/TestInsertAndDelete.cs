using System;
using System.Collections.Generic;

using NUnit.Framework;

using XData.Meta;


namespace Test
{
    public class TestInsertAndDelete
    {
        [Test]
        public void Test()
        {
            MapperConfig.HasKey<Menu>(x => x.ID);

            var model = new Menu()
            {
                ID = Guid.NewGuid(),
                Name = "测试",
                Code = "Test",
                MenuLevel = 1,
                IndexID = 1,
            };
            var db = Program.NewContext();
            db.SqlLog = Console.WriteLine;

            var row = db.Insert(model, false, x => x.RowVersion);

            var row2 = db.Delete(model);

            Assert.IsTrue(row);
            Assert.IsTrue(row2);
        }
        [Test]
        public void Test2()
        {
            var guid = Guid.NewGuid();
            var model = new Menu()
            {
                ID = guid,
                Name = "测试",
                Code = "Test",
                MenuLevel = 1,
                IndexID = 1,
            };
            var db = Program.NewContext();

            var row = db.Insert(model, x => x.Name, x => x.ID, x => x.Code, x => x.IndexID, x => x.MenuLevel, x => x.IsDeleted);

            var row2 = db.Delete(model, x => x.ID);

            Assert.IsTrue(row);
            Assert.IsTrue(row2);
        }

        [Test]
        public void Test3()
        {
            var guid = Guid.NewGuid();
            var dic = new Dictionary<string, object>()
            {
                {"ID",guid },
                {"Name", "测试"},
                {"Code", "Test"},
                {"MenuLevel", 1},
                {"IndexID",1},
                {"IsDeleted",true}
            };
            var db = Program.NewContext();

            var row = db.Insert<Menu>(dic);

            var row2 = db.Delete<Menu>(x => x.ID == guid);

            Assert.IsTrue(row);
            Assert.AreEqual(row2, 1);
        }
        [Test]
        public void Test4()
        {
            MapperConfig.HasKey<Menu>(x => x.ID);
            MapperConfig.IgnoreColumn<Menu>(x => x.RowVersion);

            var model = new Menu()
            {
                ID = Guid.NewGuid(),
                Name = "测试",
                Code = "Test",
                MenuLevel = 1,
                IndexID = 1,
            };
            var db = Program.NewContext();
            db.SqlLog = Console.WriteLine;

            var row = db.Insert(model);

            var row2 = db.Delete(model);

            Assert.IsTrue(row);
            Assert.IsTrue(row2);
        }

        [Test]
        public void TestInsertIdentity()
        {
            MapperConfig.HasKeyAndIdentity<MyTable>(x => x.ID);

            var m = new MyTable() { Name = "Test" };

            var db = Program.NewContext();

            db.Insert<MyTable>(m);
            Console.WriteLine(m.ID);
        }
    }
}