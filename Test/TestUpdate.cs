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
            MapperConfig.HasKey<Menu>(x => x.ID);
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
            db.Update(menu, false, x => x.RowVersion);

            menu = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu.Name);


            db.Update<Menu>(new Dictionary<string, object> { { "Name", name } }, x => x.ID == id);

            var menu2 = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu2.Name);

            menu2.Name = "改啊啊改噶";
            db.Update(menu2, x => x.ID == id, true, x => x.Name);

            menu2 = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu2.Name);

            menu2.Name = "目录管理";
            db.Update(menu, menu2);

            menu2 = db.Query<Menu>().Where(x => x.ID == id).ToList().FirstOrDefault();
            Console.WriteLine(menu2.Name);
        }
    }
}