using System;
using System.Reflection;

namespace XData.Common.Fast
{
    internal class CreatorWrapper<T> : ICreator
    {
        private readonly Func<object[], T> creator;

        public CreatorWrapper(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
                throw new ArgumentNullException(nameof(constructorInfo));

            creator = EmitHelper.CreateCreatorHandler<T>(constructorInfo);
        }

        public T Create(params object[] parameters)
        {
            return creator(parameters);
        }
        object ICreator.Create(params object[] parameters)
        {
            return creator(parameters);
        }
    }
}