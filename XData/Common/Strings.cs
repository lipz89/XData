using System.Collections.Generic;
using System.Linq;

namespace XData.Common
{
    /// <summary>
    /// 字符串拼接
    /// </summary>
    internal class Strings : List<string>
    {
        private readonly string separator;

        public Strings(string separator = ",")
        {
            this.separator = separator ?? ",";
        }

        public override string ToString()
        {
            if (this.Any())
            {
                return string.Join(this.separator, this);
            }
            return string.Empty;
        }
    }
}
