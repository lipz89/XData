using System;

namespace Test
{
    public interface IRowVersion
    {
        byte[] RowVersion { get; set; }
    }

    public class Dictionary : IRowVersion
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsSys { get; set; }
        public string Memo { get; set; }
        public string Status { get; set; }

        public byte[] RowVersion { get; set; }
    }
}