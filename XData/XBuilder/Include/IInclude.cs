using System.Collections.Generic;

namespace XData.XBuilder
{
    internal interface IInclude<T>
    {
        List<T> Invoke(List<T> list);
        T Invoke(T item);
    }
}