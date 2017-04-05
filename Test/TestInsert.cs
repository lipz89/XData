using System;

using NUnit.Framework;

using Winning.SPD.SCM.Domain;

using XData;
using XData.Meta;


namespace Test
{
    public class TestInsert
    {
        [Test]
        public void Test()
        {
            MetaConfig.MetaKey<Menu>(x => x.ID);

            var model = new Menu()
            {
                ID = Guid.NewGuid(),
                Name = "测试",
                Code = "Test",
                MenuLevel = 1,
                IndexID = 1,
            };
            var db = new XContext(Program.SqlConnectionString, Program.SqlProvider);

            var row = db.Insert(model, false, x => x.RowVersion).Execute();
            //var row = db.Insert(model, x => x.Name, x => x.ID, x => x.Code, x => x.IndexID, x => x.MenuLevel, x => x.IsDeleted).Execute();

            var row2 = db.Delete(model).Execute();

            Console.WriteLine(row);
            Console.WriteLine(row2);
        }
    }
}