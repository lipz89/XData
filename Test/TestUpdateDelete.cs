using System;
using System.Collections.Generic;
using NUnit.Framework;
using Test.Models;

namespace Test
{
    public class TestUpdateDelete : BaseTest
    {
        [Test]
        public void TestUpdate()
        {
            var id = Guid.NewGuid();
            var model = new Menu()
            {
                Id = id,
                Name = "≤‚ ‘",
                Code = "TestUpdate",
                MenuLevel = 100,
                IndexId = 1,
            };
            var row = Context.Insert(model);

            Assert.IsTrue(row);

            var menu = Context.GetFirstOrDefault<Menu>(x => x.Id == id);

            Assert.IsNotNull(menu);

            var name = "≤‚ ‘^^";
            menu.Name = name;
            var flag = Context.Update(menu, false, x => x.Code, x => x.Url, x => x.IndexId, x => x.Memo, x => x.MenuLevel);

            Assert.IsTrue(flag);

            menu = Context.Query<Menu>().Where(x => x.Id == id).FirstOrDefault();
            Assert.AreEqual(menu.Name, name);


            name = "≤‚ ‘^^asdfasdfasdf";
            var rowint = Context.Update<Menu>(new Dictionary<string, object> { { "Name", name } }, x => x.Id == id);
            Assert.AreEqual(rowint, 1);

            menu = Context.Query<Menu>().Where(x => x.Id == id).FirstOrDefault();
            Assert.AreEqual(menu.Name, name);

            name = "∏ƒ∞°∞°∏ƒ∏¡";
            menu.Name = name;
            rowint = Context.Update(menu, x => x.Id == id, true, x => x.Name);
            Assert.AreEqual(rowint, 1);

            menu = Context.Query<Menu>().Where(x => x.Id == id).FirstOrDefault();
            Assert.AreEqual(menu.Name, name);

            var menu2 = new Menu() { Code = "TestUpdate", MenuLevel = 100, IndexId = 1, Name = "ƒø¬ºπ‹¿Ì" };
            flag = Context.Update(menu, menu2);
            Assert.IsTrue(flag);

            menu = Context.Query<Menu>().Where(x => x.Id == id).FirstOrDefault();
            Assert.AreEqual(menu.Name, menu2.Name);

            var flag2 = Context.DeleteByKey<Menu>(id);
            Assert.IsTrue(flag2);
        }
        [Test]
        public void TestDelete()
        {
            var id = Guid.NewGuid();
            var model = new Menu()
            {
                Id = id,
                Name = "≤‚ ‘",
                Code = "TestUpdate",
                MenuLevel = 100,
                IndexId = 1,
            };
            var row = Context.Insert(model);

            Assert.IsTrue(row);

            var menu = Context.GetFirstOrDefault<Menu>(x => x.Id == id);
            Assert.IsNotNull(menu);
            var flag2 = Context.Delete(menu);
            Assert.IsTrue(flag2);

            row = Context.Insert(model);

            Assert.IsTrue(row);

            flag2 = Context.DeleteByKey<Menu>(id);
            Assert.IsTrue(flag2);
        }
    }
}