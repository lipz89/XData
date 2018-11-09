using System;
using System.Collections.Generic;

namespace Test.Models
{
    public class Menu : Entity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Guid? ParentId { get; set; }
        public int MenuLevel { get; set; }
        public int IndexId { get; set; }
        public string Memo { get; set; }
        public List<Function> Functions { get; set; }
    }
}