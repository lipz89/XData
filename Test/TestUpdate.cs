using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using XData.Meta;

namespace Test
{
    public class TestUpdate
    {
        public TestUpdate()
        {
            MapperConfig.IgnoreColumn<Menu>(x => x.RowVersion);
            MapperConfig.HasKey<Menu>(x => x.ID);
        }
        [Test]
        public void Test()
        {
            var db = Program.NewContext();

            var id = Guid.NewGuid();
            var model = new Menu()
            {
                ID = id,
                Name = "测试",
                Code = "TestUpdate",
                MenuLevel = 100,
                IndexID = 1,
            };
            var row = db.Insert(model);

            Assert.IsTrue(row);


            var menu = db.GetFirstOrDefault<Menu>(x => x.ID == id);

            Assert.IsNotNull(menu);
            db.SqlLog = Console.WriteLine;

            var name = "测试^^";
            menu.Name = name;
            var flag = db.Update(menu, false, x => x.Code, x => x.Action, x => x.Controller, x => x.IndexID, x => x.Memo, x => x.MenuLevel);

            Assert.IsTrue(flag);

            menu = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Assert.AreEqual(menu.Name, name);

            var rowint = db.Update<Menu>(new Dictionary<string, object> { { "Name", name } }, x => x.ID == id);
            Assert.AreEqual(rowint, 1);

            menu = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Assert.AreEqual(menu.Name, name);

            name = "改啊啊改噶";
            menu.Name = name;
            rowint = db.Update(menu, x => x.ID == id, true, x => x.Name);
            Assert.AreEqual(rowint, 1);

            menu = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Assert.AreEqual(menu.Name, name);

            var menu2 = new Menu() { Code = "TestUpdate", MenuLevel = 100, IndexID = 1, Name = "目录管理" };
            flag = db.Update(menu, menu2);
            Assert.IsTrue(flag);

            menu = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Assert.AreEqual(menu.Name, menu2.Name);
        }
    }
}