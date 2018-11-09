using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Test.Models;

namespace Test
{
    public class TestInsert : BaseTest
    {
        [Test, Order(0)]
        public void InsertUsers()
        {
            var users = new List<User>()
            {
                new User() {Id = Guid.NewGuid(),Code = "Admin",IsAdmin = true,UserName = "Admin",Password = "Admin"},
                new User() {Id = Guid.NewGuid(),Code = "User1",UserName = "zhangsan",Password = "123456"},
                new User() {Id = Guid.NewGuid(),Code = "User2",UserName = "lisi",Password = "123456"},
                new User() {Id = Guid.NewGuid(),Code = "User3",UserName = "wangwu",Password = "123456"},
                new User() {Id = Guid.NewGuid(),Code = "User4",UserName = "zhaoliu",Password = "123456"},
                new User() {Id = Guid.NewGuid(),Code = "User5",UserName = "woshishei",Password = "123456"}
            };
            var row = Context.Insert<User>(users);
            Console.WriteLine(row);
        }
        [Test, Order(1)]
        public void InsertRoles()
        {
            var roles = new List<Role>()
            {
                new Role() {Id = Guid.NewGuid(),Code = "Admin",Name = "管理员"},
                new Role() {Id = Guid.NewGuid(),Code = "Operator",Name = "操作员"},
                new Role() {Id = Guid.NewGuid(),Code = "Daziyuan",Name = "打字员"},
                new Role() {Id = Guid.NewGuid(),Code = "Shouyinyuan",Name = "收银员"},
                new Role() {Id = Guid.NewGuid(),Code = "Lihuoyuan",Name = "理货员"},
                new Role() {Id = Guid.NewGuid(),Code = "Yunshuyuan",Name = "运输员"}
            };
            var row = Context.Insert<Role>(roles);
            Console.WriteLine(row);
        }
        [Test, Order(2)]
        public void InsertMenus()
        {
            var idUsers = Guid.NewGuid();
            var idRoles = Guid.NewGuid();
            var idMenus = Guid.NewGuid();
            var menus = new List<Menu>()
            {
                new Menu() {Id = Guid.NewGuid(),Code = "Home",Name = "首页",IndexId = 0},
                new Menu() {Id = idUsers,Code = "Users",Name = "用户",IndexId = 1},
                new Menu() {Id = idRoles,Code = "Roles",Name = "角色",IndexId = 2},
                new Menu() {Id = idMenus,Code = "Menus",Name = "菜单",IndexId = 3},
                new Menu() {Id = Guid.NewGuid(),Code = "AddUser",Name = "添加用户",MenuLevel = 1,ParentId = idUsers,IndexId = 0},
                new Menu() {Id = Guid.NewGuid(),Code = "EditUser",Name = "编辑用户",MenuLevel = 1,ParentId = idUsers,IndexId = 1},
                new Menu() {Id = Guid.NewGuid(),Code = "DeleteUser",Name = "删除用户",MenuLevel = 1,ParentId = idUsers,IndexId = 2},
                new Menu() {Id = Guid.NewGuid(),Code = "AddRole",Name = "添加角色",MenuLevel = 1,ParentId = idRoles,IndexId = 0},
                new Menu() {Id = Guid.NewGuid(),Code = "EditRole",Name = "编辑角色",MenuLevel = 1,ParentId = idRoles,IndexId = 1},
                new Menu() {Id = Guid.NewGuid(),Code = "DeleteRole",Name = "删除角色",MenuLevel = 1,ParentId = idRoles,IndexId = 2},
                new Menu() {Id = Guid.NewGuid(),Code = "AddMenu",Name = "添加菜单",MenuLevel = 1,ParentId = idMenus,IndexId = 0},
                new Menu() {Id = Guid.NewGuid(),Code = "EditMenu",Name = "编辑菜单",MenuLevel = 1,ParentId = idMenus,IndexId = 1},
                new Menu() {Id = Guid.NewGuid(),Code = "DeleteMenu",Name = "删除菜单",MenuLevel = 1,ParentId = idMenus,IndexId = 2},
                new Menu() {Id = Guid.NewGuid(),Code = "AddAction",Name = "添加功能",MenuLevel = 1,ParentId = idMenus,IndexId = 3},
                new Menu() {Id = Guid.NewGuid(),Code = "EditAction",Name = "编辑功能",MenuLevel = 1,ParentId = idMenus,IndexId = 4},
                new Menu() {Id = Guid.NewGuid(),Code = "DeleteAction",Name = "删除功能",MenuLevel = 1,ParentId = idMenus,IndexId = 5},
            };


            foreach (var menu in menus)
            {
                if (menu.ParentId == null)
                {
                    menu.Url = "/" + menu.Code;
                }
                else
                {
                    var p = menus.FirstOrDefault(x => x.Id == menu.ParentId.Value);
                    menu.Url = $"/{p.Code}/{menu.Code}";
                }
            }
            var row = Context.Insert<Menu>(menus);
            Console.WriteLine(row);
        }
        [Test, Order(3)]
        public void InsertActions()
        {
            var menus = Context.Query<Menu>().Where(x => x.ParentId.HasValue);
            Console.WriteLine(menus.Count());

            var funcs = menus.Select(x =>
                new Function() { MenuId = x.Id, Code = x.Code, Name = x.Name }).ToList();
            funcs.ForEach(x => x.Id = Guid.NewGuid());
            //var row = ctx.Insert<Function>(funcs);
            //Console.WriteLine(row);
        }
        [Test, Order(4)]
        public void InsertUserRoles()
        {
            var users = Context.Query<User>().ToList();
            var roles = Context.Query<Role>().ToList();

            var userRoles = new List<UserRole>();
            foreach (var user in users)
            {
                if (user.IsAdmin)
                {
                    var urs = roles.Where(x => x.Code == "Admin").Select(x => new UserRole()
                    {
                        Id = Guid.NewGuid(),
                        RoleId = x.Id,
                        UserId = user.Id
                    });
                    userRoles.AddRange(urs);
                }
                else
                {
                    var urs = roles.Where(x => x.Code != "Admin").Select(x => new UserRole()
                    {
                        Id = Guid.NewGuid(),
                        RoleId = x.Id,
                        UserId = user.Id
                    });
                    userRoles.AddRange(urs);
                }
            }

            var row = Context.Insert<UserRole>(userRoles);
            Console.WriteLine(row);
        }
        [Test, Order(5)]
        public void InsertRoleActions()
        {
            var actions = Context.Query<Function>().ToList();
            var roles = Context.Query<Role>().ToList();

            var roleActions = new List<RoleFunction>();
            var rd = new Random(DateTime.Now.Millisecond);
            var flag = true;
            foreach (var role in roles)
            {
                foreach (var function in actions)
                {
                    if (role.Code != "Admin")
                    {
                        flag = true;
                    }
                    else
                    {
                        var i = rd.Next(100);
                        flag = i > 80;
                    }

                    if (flag)
                    {
                        var rf = new RoleFunction() { Id = Guid.NewGuid(), FunctionId = function.Id, RoleId = role.Id };
                        roleActions.Add(rf);
                    }
                }
            }
            var row = Context.Insert<RoleFunction>(roleActions);
            Console.WriteLine(row);
        }
    }
}