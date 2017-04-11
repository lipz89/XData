namespace XData.Extentions
{
    internal static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        /// <summary>
        /// 判断目标字符串和当前字符串是否代表同一个字段，
        /// 比较规则：大小写忽略，前后缀_忽略，然后判等
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsSameField(this string source, string value)
        {
            if (source.IsNullOrWhiteSpace() || value.IsNullOrWhiteSpace())
            {
                return false;
            }
            return source.Trim().Trim('_').ToLower() == value.Trim().Trim('_').ToLower();
        }
    }
}