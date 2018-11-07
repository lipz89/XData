using System;

namespace Test
{
    public class UserRole : Entity
    {
        public Guid UserID { get; set; }
        public Guid RoleID { get; set; }
    }
}