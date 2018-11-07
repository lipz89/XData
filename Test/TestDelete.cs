using System;
using NUnit.Framework;

namespace Test
{
    public class TestDelete: BaseTest
    {
        [Test, Order(0)]
        public void DeleteUserRoles()
        {
            var ctx = Program.NewContext();
            var row = ctx.Delete<UserRole>(null);
            Console.WriteLine(row);
        }
        [Test, Order(1)]
        public void DeleteRoleActions()
        {
            var ctx = Program.NewContext();
            var row = ctx.Delete<RoleFunction>(x => true);
            Console.WriteLine(row);
        }
        [Test, Order(2)]
        public void DeleteUsers()
        {
            var ctx = Program.NewContext();
            var row = ctx.Delete<User>(x => true);
            Console.WriteLine(row);
        }
        [Test, Order(3)]
        public void DeleteRoles()
        {
            var ctx = Program.NewContext();
            var row = ctx.Delete<Role>(x => true);
            Console.WriteLine(row);
        }
        [Test, Order(4)]
        public void DeleteActions()
        {
            var ctx = Program.NewContext();
            var row = ctx.Delete<Function>(x => true);
            Console.WriteLine(row);
        }
        [Test, Order(5)]
        public void DeleteMenus()
        {
            var ctx = Program.NewContext();
            var row = ctx.Delete<Menu>(x => true);
            Console.WriteLine(row);
        }
    }
}