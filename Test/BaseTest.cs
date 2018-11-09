using NUnit.Framework;
using Test.Models;
using XData;
using XData.Meta;

namespace Test
{
    public class BaseTest
    {
        protected readonly XContext Context;

        public BaseTest()
        {
            Context = Program.NewContext();
        }

        [OneTimeSetUp]
        public void Init()
        {
            MapperConfig.HasTableName<UserRole>("UserRoles");
            MapperConfig.HasTableName<RoleFunction>("RoleActions");
            MapperConfig.HasTableName<Role>("Roles");
            MapperConfig.HasTableName<User>("Users");
            MapperConfig.HasTableName<Function>("Actions");
            MapperConfig.HasTableName<Menu>("Menus");
            MapperConfig.HasTableName<SomeValues>("SomeValues");
            MapperConfig.HasTableName<ModelMultiKeys>("TestModel");

            MapperConfig.HasKey<UserRole>(x => x.Id);
            MapperConfig.HasKey<RoleFunction>(x => x.Id);
            MapperConfig.HasKey<Role>(x => x.Id);
            MapperConfig.HasKey<User>(x => x.Id);
            MapperConfig.HasKey<Function>(x => x.Id);
            MapperConfig.HasKey<Menu>(x => x.Id);
            MapperConfig.HasKey<SomeValues>(x => x.Id);
            MapperConfig.HasKey<ModelMultiKeys>(x => x.Name, x => x.Code);

            MapperConfig.HasColumnName<RoleFunction>(x => x.FunctionId, "ActionID");
            MapperConfig.HasColumnName<User>(x => x.IsAdmin, "IsSys");
        }
    }
}