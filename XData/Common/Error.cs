using System;

namespace XData.Common
{
    /// <summary>
    /// 异常
    /// </summary>
    internal static class Error
    {
        public static Exception ArgumentNullException(string name)
        {
            return new ArgumentNullException(name);
        }

        public static Exception ArgumentException(string message, string name)
        {
            return new ArgumentException(message, name);
        }

        public static Exception Exception(string message, Exception innerException = null)
        {
            return new Exception(message, innerException);
        }

        public static Exception NotSupportedException(string message)
        {
            return new NotSupportedException(message);
        }

        public static Exception InvalidCastException(string message)
        {
            return new InvalidCastException(message);
        }

    }
}
