using Assets.GameAssets.Scripts.UI.Helper;
using Zenject;

namespace Assets.GameAssets.Scripts.UI.Installer
{
    public class Installer: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAlgorithmsProvider>().ToSingle<AlgorithmsProvider>();
            Container.Bind<IResourceLoader>().ToTransient<ResourceLoader>();
        }
    }
}
