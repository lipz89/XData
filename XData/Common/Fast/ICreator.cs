namespace XData.Common.Fast
{
    internal interface ICreator
    {
        object Create(params object[] parameters);
    }
}