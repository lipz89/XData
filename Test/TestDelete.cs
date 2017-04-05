using System;

using NUnit.Framework;

using Winning.SPD.SCM.Domain;

using XData;


namespace Test
{
    public class TestDelete
    {
        [Test]
        public void Test()
        {
            var guid = Guid.Parse("09274cdc-cbfb-4788-af29-0b23b4443032");
            var db = new XContext(Program.SqlConnectionString, Program.SqlProvider);
            var row = db.Delete<Menu>().Where(x => x.ID == guid).Execute();
            Console.WriteLine(row);
        }
    }
}