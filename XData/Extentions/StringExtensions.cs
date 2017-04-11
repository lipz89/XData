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
        /// �ж�Ŀ���ַ����͵�ǰ�ַ����Ƿ����ͬһ���ֶΣ�
        /// �ȽϹ��򣺴�Сд���ԣ�ǰ��׺_���ԣ�Ȼ���е�
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