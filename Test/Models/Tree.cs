using System.Collections.Generic;

namespace Test
{
    public class Tree
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? ParentID { get; set; }
        public Tree Parent { get; set; }
        public ICollection<Tree> Children { get; set; }
    }
}