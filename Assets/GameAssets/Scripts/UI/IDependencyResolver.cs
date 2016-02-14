namespace Assets.GameAssets.Scripts.UI
{
    public interface IDependencyResolver
    {
        T Resolve<T>();
    }
}