using System;
using NUnit.Framework;
using Test.Models;

namespace Test
{
    public class TestDelete : BaseTest
    {
        [Test, Order(0)]
        public void DeleteUserRoles()
        {
            var row = Context.DeleteBy<UserRole>(null);
            Console.WriteLine(row);
        }
        [Test, Order(1)]
        public void DeleteRoleActions()
        {
            var row = Context.DeleteBy<RoleFunction>(null);
            Console.WriteLine(row);
        }
        [Test, Order(2)]
        public void DeleteUsers()
        {
            var row = Context.DeleteBy<User>(null);
            Console.WriteLine(row);
        }
        [Test, Order(3)]
        public void DeleteRoles()
        {
            var row = Context.DeleteBy<Role>(null);
            Console.WriteLine(row);
        }
        [Test, Order(4)]
        public void DeleteActions()
        {
            var row = Context.DeleteBy<Function>(null);
            Console.WriteLine(row);
        }
        [Test, Order(5)]
        public void DeleteMenus()
        {
            var row = Context.DeleteBy<Menu>(null);
            Console.WriteLine(row);
        }
    }
}