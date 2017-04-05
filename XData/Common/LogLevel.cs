namespace XData.Common
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 包含最详细的日志信息。这些消息可能包含敏感的应用程序数据。
        /// 这些消息都是默认禁用的,不应该在生产环境中启用。
        /// </summary>
        Trace = 1,
        /// <summary>
        /// 在开发过程中日志用于交互式跟踪。
        /// 这些日志应该主要包含有用的调试信息,没有长期的价值。
        /// </summary>
        Debug,
        /// <summary>
        /// 日志跟踪应用程序的通用流。这些日志应该长期价值。
        /// </summary>
        Information,
        /// <summary>
        /// 日志,突出应用程序流的异常或意外事件,但不会导致应用程序停止执行。
        /// </summary>
        Warning,
        /// <summary>
        /// 日志,强调在当前流程由于执行失败而停止时。这些应该显示当前活动的失败,不是一个应用程序失败。.
        /// </summary>
        Error,
        /// <summary>
        /// 日志描述一个不可恢复的应用程序或系统崩溃,或者一个灾难性故障,需要立即注意。
        /// </summary>
        Critical,
        /// <summary>
        /// 不是用来写日志消息。指定日志类别不应该写的任何消息。
        /// </summary>
        None = 2147483647
    }
}