using System.Collections.Generic;

namespace Test.Models
{
    public class Tree
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public Tree Parent { get; set; }
        public ICollection<Tree> Children { get; set; }
    }
}