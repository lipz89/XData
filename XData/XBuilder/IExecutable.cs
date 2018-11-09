namespace XData.XBuilder
{
    /// <summary>
    /// 可执行的Sql接口
    /// </summary>
    public interface IExecutable
    {
        /// <summary>
        /// 执行Sql命令
        /// </summary>
        /// <returns>返回受影响的行数</returns>
        int Execute();
    }
}