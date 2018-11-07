using System;

namespace Test
{
    public class Function : Entity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid MenuID { get; set; }
        public int IndexID { get; set; }
        public string Memo { get; set; }
        public Menu Menu { get; set; }
    }
}