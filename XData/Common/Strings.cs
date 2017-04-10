using System.Collections.Generic;
using System.Linq;

namespace XData.Common
{
    /// <summary>
    /// 字符串拼接
    /// </summary>
    internal class Strings : List<string>
    {
        private readonly string _separator;

        public Strings(string separator = ",")
        {
            this._separator = separator ?? ",";
        }

        public override string ToString()
        {
            if (this.Any())
            {
                return string.Join(this._separator, this);
            }
            return string.Empty;
        }
    }
}
