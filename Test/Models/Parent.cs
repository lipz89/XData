using System.Collections.Generic;

namespace Test.Models
{
    public class Parent
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<Child> Children { get; set; }

        public override string ToString()
        {
            return string.Format("{0}; {1}: {2}; ChilredCount:{3}", Id, Code, Name, Children?.Count ?? 0);
        }
    }
}