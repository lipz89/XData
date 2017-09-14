namespace Test
{
    public class Child
    {
        public int? ParentID { get; set; }
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Parent Parent { get; set; }

        public override string ToString()
        {
            return string.Format("{0}; {1}: {2}", ID, Code, Name);
        }
    }
}