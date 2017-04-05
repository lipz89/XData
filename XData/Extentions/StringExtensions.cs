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

        public static bool IsSameField(this string source, string value)
        {
            if (source.IsNullOrWhiteSpace() || value.IsNullOrWhiteSpace())
            {
                return false;
            }
            return source.Trim("_ ".ToCharArray()).ToLower() == value.Trim("_ ".ToCharArray()).ToLower();
        }
    }
}