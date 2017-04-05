using System.Data;

namespace XData.Core
{
    internal interface IReader<T>
    {
        T Read(IDataReader reader);
    }
}