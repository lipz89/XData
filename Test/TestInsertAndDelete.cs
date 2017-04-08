using System;
using System.Collections.Generic;

using NUnit.Framework;

using Winning.SPD.SCM.Domain;

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

            var row = db.Insert(model, false, x => x.RowVersion).Execute();

            var row2 = db.Delete(model).Execute();

            Console.WriteLine(row);
            Console.WriteLine(row2);
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

            var row = db.Insert(model, x => x.Name, x => x.ID, x => x.Code, x => x.IndexID, x => x.MenuLevel, x => x.IsDeleted).Execute();

            var row2 = db.Delete(model, x => x.ID).Execute();

            Console.WriteLine(row);
            Console.WriteLine(row2);
        }
        [Test]
        public void Test3()
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

            var row = db.Insert(model, false, x => x.RowVersion).Execute();

            var row2 = db.Delete<Menu>().Where(x => x.ID == guid).Execute();

            Console.WriteLine(row);
            Console.WriteLine(row2);
        }
        [Test]
        public void Test4()
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

            var row = db.Insert<Menu>(dic).Execute();

            var row2 = db.Delete<Menu>().Where(x => x.ID == guid).Execute();

            Console.WriteLine(row);
            Console.WriteLine(row2);
        }
    }
}