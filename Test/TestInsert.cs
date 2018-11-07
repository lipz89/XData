using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Test
{
    public class TestInsert : BaseTest
    {
        [Test, Order(0)]
        public void InsertUsers()
        {
            var users = new List<User>()
            {
                new User() {ID = Guid.NewGuid(),Code = "Admin",IsAdmin = true,UserName = "Admin",Password = "Admin"},
                new User() {ID = Guid.NewGuid(),Code = "User1",UserName = "zhangsan",Password = "123456"},
                new User() {ID = Guid.NewGuid(),Code = "User2",UserName = "lisi",Password = "123456"},
                new User() {ID = Guid.NewGuid(),Code = "User3",UserName = "wangwu",Password = "123456"},
                new User() {ID = Guid.NewGuid(),Code = "User4",UserName = "zhaoliu",Password = "123456"},
                new User() {ID = Guid.NewGuid(),Code = "User5",UserName = "woshishei",Password = "123456"}
            };
            var ctx = Program.NewContext();
            var row = ctx.Insert<User>(users);
            Console.WriteLine(row);
        }
        [Test, Order(1)]
        public void InsertRoles()
        {
            var roles = new List<Role>()
            {
                new Role() {ID = Guid.NewGuid(),Code = "Admin",Name = "管理员"},
                new Role() {ID = Guid.NewGuid(),Code = "Operator",Name = "操作员"},
                new Role() {ID = Guid.NewGuid(),Code = "Daziyuan",Name = "打字员"},
                new Role() {ID = Guid.NewGuid(),Code = "Shouyinyuan",Name = "收银员"},
                new Role() {ID = Guid.NewGuid(),Code = "Lihuoyuan",Name = "理货员"},
                new Role() {ID = Guid.NewGuid(),Code = "Yunshuyuan",Name = "运输员"}
            };
            var ctx = Program.NewContext();
            var row = ctx.Insert<Role>(roles);
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
                new Menu() {ID = Guid.NewGuid(),Code = "Home",Name = "首页",IndexID = 0},
                new Menu() {ID = idUsers,Code = "Users",Name = "用户",IndexID = 1},
                new Menu() {ID = idRoles,Code = "Roles",Name = "角色",IndexID = 2},
                new Menu() {ID = idMenus,Code = "Menus",Name = "菜单",IndexID = 3},
                new Menu() {ID = Guid.NewGuid(),Code = "AddUser",Name = "添加用户",MenuLevel = 1,ParentID = idUsers,IndexID = 0},
                new Menu() {ID = Guid.NewGuid(),Code = "EditUser",Name = "编辑用户",MenuLevel = 1,ParentID = idUsers,IndexID = 1},
                new Menu() {ID = Guid.NewGuid(),Code = "DeleteUser",Name = "删除用户",MenuLevel = 1,ParentID = idUsers,IndexID = 2},
                new Menu() {ID = Guid.NewGuid(),Code = "AddRole",Name = "添加角色",MenuLevel = 1,ParentID = idRoles,IndexID = 0},
                new Menu() {ID = Guid.NewGuid(),Code = "EditRole",Name = "编辑角色",MenuLevel = 1,ParentID = idRoles,IndexID = 1},
                new Menu() {ID = Guid.NewGuid(),Code = "DeleteRole",Name = "删除角色",MenuLevel = 1,ParentID = idRoles,IndexID = 2},
                new Menu() {ID = Guid.NewGuid(),Code = "AddMenu",Name = "添加菜单",MenuLevel = 1,ParentID = idMenus,IndexID = 0},
                new Menu() {ID = Guid.NewGuid(),Code = "EditMenu",Name = "编辑菜单",MenuLevel = 1,ParentID = idMenus,IndexID = 1},
                new Menu() {ID = Guid.NewGuid(),Code = "DeleteMenu",Name = "删除菜单",MenuLevel = 1,ParentID = idMenus,IndexID = 2},
                new Menu() {ID = Guid.NewGuid(),Code = "AddAction",Name = "添加功能",MenuLevel = 1,ParentID = idMenus,IndexID = 3},
                new Menu() {ID = Guid.NewGuid(),Code = "EditAction",Name = "编辑功能",MenuLevel = 1,ParentID = idMenus,IndexID = 4},
                new Menu() {ID = Guid.NewGuid(),Code = "DeleteAction",Name = "删除功能",MenuLevel = 1,ParentID = idMenus,IndexID = 5},
            };

            var ctx = Program.NewContext();
            var row = ctx.Insert<Menu>(menus);
            Console.WriteLine(row);
        }
        [Test, Order(3)]
        public void InsertActions()
        {
            var ctx = Program.NewContext();
            var menus = ctx.Query<Menu>().Where(x => x.ParentID.HasValue);
            Console.WriteLine(menus.Count());

            var funcs = menus.Select(x =>
                new Function() { MenuID = x.ID, Code = x.Code, Name = x.Name }).ToList();
            funcs.ForEach(x => x.ID = Guid.NewGuid());
            //var row = ctx.Insert<Function>(funcs);
            //Console.WriteLine(row);
        }
        [Test, Order(4)]
        public void InsertUserRoles()
        {
            var ctx = Program.NewContext();
            var users = ctx.Query<User>().ToList();
            var roles = ctx.Query<Role>().ToList();

            var userRoles = new List<UserRole>();
            foreach (var user in users)
            {
                if (user.IsAdmin)
                {
                    var urs = roles.Where(x => x.Code == "Admin").Select(x => new UserRole()
                    {
                        ID = Guid.NewGuid(),
                        RoleID = x.ID,
                        UserID = user.ID
                    });
                    userRoles.AddRange(urs);
                }
                else
                {
                    var urs = roles.Where(x => x.Code != "Admin").Select(x => new UserRole()
                    {
                        ID = Guid.NewGuid(),
                        RoleID = x.ID,
                        UserID = user.ID
                    });
                    userRoles.AddRange(urs);
                }
            }

            var row = ctx.Insert<UserRole>(userRoles);
            Console.WriteLine(row);
        }
        [Test, Order(5)]
        public void InsertRoleActions()
        {
            var ctx = Program.NewContext();
            var actions = ctx.Query<Function>().ToList();
            var roles = ctx.Query<Role>().ToList();

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
                        var rf = new RoleFunction() { ID = Guid.NewGuid(), FunctionID = function.ID, RoleID = role.ID };
                        roleActions.Add(rf);
                    }
                }
            }
            var row = ctx.Insert<RoleFunction>(roleActions);
            Console.WriteLine(row);
        }
    }
}