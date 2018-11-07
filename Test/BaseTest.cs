using NUnit.Framework;
using XData.Meta;

namespace Test
{
    public class BaseTest
    {
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

            MapperConfig.HasKey<UserRole>(x => x.ID);
            MapperConfig.HasKey<RoleFunction>(x => x.ID);
            MapperConfig.HasKey<Role>(x => x.ID);
            MapperConfig.HasKey<User>(x => x.ID);
            MapperConfig.HasKey<Function>(x => x.ID);
            MapperConfig.HasKey<Menu>(x => x.ID);
            MapperConfig.HasKey<SomeValues>(x => x.ID);

            MapperConfig.HasColumnName<RoleFunction>(x => x.FunctionID, "ActionID");
            MapperConfig.HasColumnName<User>(x => x.IsAdmin, "IsSys");
        }
    }
}