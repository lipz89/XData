using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Winning.SPD.SCM.Domain;

using XData.Meta;

namespace Test
{
    public class TestUpdate
    {
        public TestUpdate()
        {
            MapperConfig.IgnoreColumn<Menu>(x => x.RowVersion);
            //MapperConfig.HasKey<Menu>(x => x.ID);
        }
        [Test]
        public void Test()
        {
            var db = Program.NewContext();
            var menu = db.Query<Menu>().OrderBy(x => x.Action).ToList().FirstOrDefault();

            var id = menu.ID;
            Console.WriteLine(id);
            Console.WriteLine(menu.Name);

            var name = menu.Name + "^^";

            menu.Name += "_";
            db.Update(menu, false, x => x.RowVersion).Where(x => x.ID == id).Execute();

            menu = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu.Name);


            db.Update<Menu>(new Dictionary<string, object> { { "Name", name } }).Where(x => x.ID == id).Execute();

            var menu2 = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu2.Name);

            menu2.Name = "改啊啊改噶";
            db.Update(menu2, x => x.Name).Where(x => x.ID == id).Execute();

            menu2 = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu2.Name);

            menu2.Name = "目录管理";
            db.Update(menu, menu2, x => x.ID).Execute();

            menu2 = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu2.Name);
        }
        [Test]
        public void Test2()
        {
            var db = Program.NewContext();
            var menu = db.Query<Menu>().ToList().FirstOrDefault();

            var id = menu.ID;
            Console.WriteLine(id);
            Console.WriteLine(menu.Name);

            var name = menu.Name + "^^";
            
            db.Update<Menu>(new Dictionary<string, object> { { "Name", name } }).Where(x => x.ID == id).Execute();

            var menu2 = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu2.Name);

            menu2.Name = "改啊啊改噶";

            db.Update(menu, menu2, x => x.ID).Execute();

            menu2 = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu2.Name);


            menu.Name += "_";
            db.Update(menu, false, x => x.RowVersion).Execute();

            menu = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu.Name);

            menu2.Name = "机构管理";
            db.Update(menu2, x => x.Name).Execute();

            menu2 = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu2.Name);
        }
    }
}