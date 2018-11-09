using System;

namespace Test.Models
{
    public class RoleFunction : Entity
    {
        public Guid FunctionId { get; set; }
        public Guid RoleId { get; set; }
    }
}