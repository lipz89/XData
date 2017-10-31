using System;

namespace Test
{
    public class Detail : IRowVersion
    {
        public Detail()
        {
            ID = Guid.NewGuid();
            DictionaryID = Guid.Parse("E6E3763F-A8DF-491A-8CAC-191B228D7624");
        }
        public Guid ID { get; set; }
        public string code { get; set; }
        public string address { get; set; }
        public string pcode { get; set; }
        public string memo { get; set; }
        public Guid DictionaryID { get; set; }
        public int IndexID { get; set; }
        public Guid? ParentID { get; set; }

        public byte[] RowVersion { get; set; }
    }
}