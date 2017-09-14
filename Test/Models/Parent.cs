using System.Collections.Generic;

namespace Test
{
    public class Parent
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<Child> Children { get; set; }

        public override string ToString()
        {
            return string.Format("{0}; {1}: {2}; ChilredCount:{3}", ID, Code, Name, Children?.Count ?? 0);
        }
    }
}