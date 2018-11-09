using System;

namespace Test.Models
{
    public class Function : Entity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid MenuId { get; set; }
        public int IndexId { get; set; }
        public string Memo { get; set; }
        public Menu Menu { get; set; }
    }
}