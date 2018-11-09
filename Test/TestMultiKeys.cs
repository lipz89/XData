using System;
using System.Linq;
using NUnit.Framework;
using Test.Models;

namespace Test
{
    public class TestMultiKeys : BaseTest
    {
        [OneTimeSetUp]
        public void Init2()
        {
        }

        [Test, Order(0)]
        public void Insert()
        {
            Context.DeleteBy<ModelMultiKeys>(null);
            var model = new ModelMultiKeys
            {
                Name = "Name",
                Code = "Code",
                Url = "Url",
                Memo = "Memo"
            };
            var model2 = new ModelMultiKeys
            {
                Name = "Name2",
                Code = "Code",
                Url = "Url2",
                Memo = "Memo2"
            };
            var model3 = new ModelMultiKeys
            {
                Name = "Name",
                Code = "Code2",
                Url = "Url3",
                Memo = "Memo3"
            };


            var row = Context.Insert<ModelMultiKeys>(model, model2, model3);
            Assert.AreEqual(row, 3);

            var models = Enumerable.Range(0, 10).Select(x => new ModelMultiKeys
            {
                Name = "Name3",
                Code = "__Code" + x,
                Url = "Patch",
                Memo = "Patch"
            }).ToList();
            row = Context.Insert<ModelMultiKeys>(models);
            Assert.AreEqual(row, models.Count);

            var model4 = new ModelMultiKeys
            {
                Name = "Name",
                Code = "Code3",
                Url = "Url",
                Memo = "Memo"
            };
            var flag = Context.Insert<ModelMultiKeys>(model4);
            Assert.IsTrue(flag);

            Assert.Throws<XData.Common.XDataException>(() =>
            {
                var flag2 = Context.Insert<ModelMultiKeys>(model4);
            });
        }
        [Test, Order(3)]
        public void Query()
        {
            var query = Context.Query<ModelMultiKeys>();
            var list = query.ToList();
            Console.WriteLine(list.Count);

            var item = query.Where(x => x.Code == "Code2").FirstOrDefault();
            var item2 = query.FirstOrDefault(x => x.Code == "Code2");

            var item3 = Context.GetFirstOrDefault<ModelMultiKeys>();
            var item4 = Context.GetFirstOrDefault<ModelMultiKeys>(x => x.Code == "Code2");

            var bykey = Context.GetByKey<ModelMultiKeys>(item.Name, item.Code);
        }

        [Test, Order(2)]
        public void Update()
        {
            var model4 = new ModelMultiKeys
            {
                Name = "Name",
                Code = "Code2",
                Url = "Url22222",
                Memo = "Memo333333"
            };
            var flag = Context.Update(model4);

            var model5 = new ModelMultiKeys
            {
                Name = "Name",
                Code = "Code2",
                Url = "Url22222asdfasdfadsf",
                Memo = "Memo33333fagafgasdf3"
            };
            flag = Context.Update(model4, model5);
        }

        [Test, Order(1)]
        public void Delete()
        {
            var model4 = new ModelMultiKeys
            {
                Name = "Name",
                Code = "Code3"
            };
            var flag = Context.Delete(model4);
            Assert.IsTrue(flag);

            flag = Context.DeleteByKey<ModelMultiKeys>("Name2", "Code");
            Assert.IsTrue(flag);

            var row = Context.DeleteBy<ModelMultiKeys>(x => x.Name == "Name3");
            Assert.IsTrue(row > 0);
        }
    }
}