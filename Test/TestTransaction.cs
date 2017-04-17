using System;

using NUnit.Framework;

using Winning.SPD.SCM.Domain;

using XData;
using XData.Meta;

namespace Test
{
    public class TestTransaction
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
            {
                db.BeginTransaction();

                var row = db.Insert(model, false, x => x.RowVersion);
                Console.WriteLine(row);

                db.CompleteTransaction();

                db.BeginTransaction();
                model.ID = Guid.NewGuid();
                var row2 = db.Insert(model, false, x => x.RowVersion);
                Console.WriteLine(row2);

                db.CompleteTransaction();
            }
        }
        [Test]
        public void Test2()
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
            using (var db = Program.NewContext())
            {
                db.BeginTransaction();

                var row = db.Insert(model, false, x => x.RowVersion);
                var row2 = db.Delete(model);
                Console.WriteLine(row);
                Console.WriteLine(row2);

                db.CompleteTransaction();
            }
        }
    }
}