using System;

namespace Test
{
    public class Menu 
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public int IndexID { get; set; }
        public string Memo { get; set; }
        public int MenuLevel { get; set; }
        public bool IsDeleted { get; set; }

        public byte[] RowVersion { get; set; }
    }
}