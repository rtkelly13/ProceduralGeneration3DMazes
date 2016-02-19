using Assets.GameAssets.Scripts.UI.Helper;
using Assets.GameAssets.Scripts.UI.Menu.Settings;
using Assets.GameAssets.Scripts.UI.Menu.Settings.GrowingTree;
using Assets.GameAssets.Scripts.UI.Menu.Validation;
using Zenject;

namespace Assets.GameAssets.Scripts.UI.Installer
{
    public class Installer: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAlgorithmsProvider>().ToSingle<AlgorithmsProvider>();
            Container.Bind<IResourceLoader>().ToTransient<ResourceLoader>();
            Container.Bind<ISettingBuilder>().ToSingle<GrowingTreeSettingsBuilder>();
            Container.Bind<ISettingBuilder>().ToSingle<NoneSettingsBuilder>();
            Container.Bind<ISettingsBuilder>().ToSingle<SettingsBuilder>();

            Container.Bind<IBaseValidateSettings>().ToTransient<BaseValidateSettings>();
            Container.Bind<IValidateSetting>().ToTransient<GrowingTreeAlgorithmValidator>();
            Container.Bind<IValidateSetting>().ToTransient<NoneAlgorithmValidator>();

            Container.Bind<IAlgorithmSettingsInitialiser>().ToTransient<AlgorithmSettingsInitialiser>();
            Container.Bind<IValidationRetriever>().ToTransient<ValidationRetriever>();
            Container.Bind<IValidateSettings>().ToTransient<ValidateSettings>();

            Container.Bind<IGrowingTreeStrategyStorage>().ToTransient<GrowingTreeStrategyStorage>();
            Container.Bind<IModelOptionsProvider>().ToTransient<ModelOptionsProvider>();

        }
    }
}
