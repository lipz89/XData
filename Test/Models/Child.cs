namespace Test.Models
{
    public class Child
    {
        public int? ParentId { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Parent Parent { get; set; }

        public override string ToString()
        {
            return string.Format("{0}; {1}: {2}", Id, Code, Name);
        }
    }
}