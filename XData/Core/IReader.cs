using System.Data;

namespace XData.Core
{
    internal interface IReader<out T>
    {
        T Read(IDataReader reader);
    }
}