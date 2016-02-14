using System;

namespace Assets.GameAssets.Scripts.UI
{
    public class DependencyResolver: IDependencyResolver
    {
        public T Resolve<T>()
        {
            if (typeof(T) == typeof(IAlgorithmsProvider))
            {
                return (T) (new AlgorithmsProvider() as IAlgorithmsProvider);
            }
            throw new ArgumentException("Invalid arguement");
        }
    }
}