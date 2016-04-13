using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Heuristics;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.Maze.Solver;
using Assets.GameAssets.Scripts.MazeLoading;
using Assets.GameAssets.Scripts.MazeUI;
using Assets.GameAssets.Scripts.MazeUI.ImageHandling;
using Assets.GameAssets.Scripts.MazeUI.UserManagement;
using Assets.GameAssets.Scripts.UI.Helper;
using Assets.GameAssets.Scripts.UI.Menu.Settings;
using Assets.GameAssets.Scripts.UI.Menu.Settings.GrowingTree;
using Assets.GameAssets.Scripts.UI.Menu.Validation;
using Zenject;
using AlgorithmsProvider = Assets.GameAssets.Scripts.Maze.Helper.AlgorithmsProvider;

namespace Assets.GameAssets.Scripts
{
    public class Installer: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IMovementHelper>().ToTransient<MovementHelper>();
            Container.Bind<IDirectionsFlagParser>().ToTransient<DirectionsFlagParser>();
            Container.Bind<IMazePointFactory>().ToTransient<MazePointFactory>();
            Container.Bind<IPointValidity>().ToTransient<PointValidity>();
            Container.Bind<IRandomPointGenerator>().ToTransient<RandomPointGenerator>();
            Container.Bind<IRandomValueGenerator>().ToTransient<RandomValueGenerator>();
            Container.Bind<ILoadCellPrefab>().ToTransient<LoadCellPrefab>();
            Container.Bind<ISpriteCache>().ToSingle<SpriteCache>();
            Container.Bind<ICellInformationLoader>().ToTransient<CellInformationLoader>();
            Container.Bind<ICurrentSettingsHolder>().ToTransient<CurrentSettingsHolder>();
            Container.Bind<IMazeUiBuilder>().ToTransient<MazeUiBuilder>();
            Container.Bind<IInputHandler>().ToTransient<InputHandler>();
            Container.Bind<IMazeHelper>().ToTransient<MazeHelper>();
            Container.Bind<ICurrentMazeHolder>().ToTransient<CurrentMazeHolder>();

            Container.Bind<IAlgorithmsProvider>().ToSingle<UI.Helper.AlgorithmsProvider>();
            Container.Bind<IResourceLoader>().ToTransient<ResourceLoader>();
            Container.Bind<ISettingBuilder>().ToSingle<GrowingTreeSettingsBuilder>();
            Container.Bind<ISettingBuilder>().ToSingle<NoneSettingsBuilder>();
            Container.Bind<ISettingBuilder>().ToSingle<RecursiveBacktrackerSettingsBuilder>();
            Container.Bind<ISettingBuilder>().ToSingle<BinaryTreeSettingsBuilder>();
            Container.Bind<ISettingsBuilder>().ToSingle<SettingsBuilder>();
            Container.Bind<IBaseValidateSettings>().ToTransient<BaseValidateSettings>();
            Container.Bind<IValidateSetting>().ToTransient<RecursiveBacktrackerValidator>();
            Container.Bind<IValidateSetting>().ToTransient<GrowingTreeAlgorithmValidator>();
            Container.Bind<IValidateSetting>().ToTransient<NoneAlgorithmValidator>();
            Container.Bind<IValidateSetting>().ToTransient<BinaryTreeValidator>();
            Container.Bind<IAlgorithmSettingsInitialiser>().ToTransient<AlgorithmSettingsInitialiser>();
            Container.Bind<IValidationRetriever>().ToTransient<ValidationRetriever>();
            Container.Bind<IValidateSettings>().ToTransient<ValidateSettings>();
            Container.Bind<IGrowingTreeStrategyStorage>().ToTransient<GrowingTreeStrategyStorage>();
            Container.Bind<IModelOptionsProvider>().ToTransient<ModelOptionsProvider>();
            Container.Bind<IYesNoOptionsProvider>().ToTransient<YesNoOptionsProvider>();
            Container.Bind<IWallCarverOptionsProvider>().ToTransient<WallCarverOptionsProvider>();
            Container.Bind<IPointsAndDirectionsRetriever>().ToTransient<PointsAndDirectionsRetriever>();

            Container.Bind<ISceneLoader>().ToTransient<SceneLoader>();
            Container.Bind<IMazeGenerationFactory>().ToTransient<MazeGenerationFactory>();
            Container.Bind<IMazeModelFactory>().ToTransient<MazeModelFactory>();
            Container.Bind<IBinaryTreeAlgorithm>().ToTransient<BinaryTreeAlgorithm>();
            Container.Bind<IMazeFactory>().ToTransient<MazeFactory>();
            Container.Bind<IMazeValidator>().ToTransient<MazeValidator>();
            Container.Bind<IMazeArrayBuilder>().ToTransient<MazeArrayBuilder>();
            Container.Bind<IDeadEndModelWrapperFactory>().ToTransient<DeadEndModelWrapperFactory>();
            Container.Bind<IDeadEndFiller>().ToTransient<DeadEndFiller>();
            Container.Bind<IModelsWrapperFactory>().ToTransient<ModelsWrapperFactory>();
            Container.Bind<IRandomCarver>().ToTransient<RandomCarver>();
            Container.Bind<IExtraWallCalculator>().ToTransient<ExtraWallCalculator>();
            Container.Bind<IGenerateTestCase>().ToTransient<GenerateTestCase>();
            Container.Bind<IDoorwayLoader>().ToTransient<DoorwayLoader>();
            Container.Bind<ICircleLoader>().ToTransient<CircleLoader>();
            Container.Bind<IMazeNeedsGenerating>().ToTransient<MazeNeedsGenerating>();
            Container.Bind<IHeuristicsGenerator>().ToTransient<HeuristicsGenerator>();
            Container.Bind<IModelStateHelper>().ToTransient<ModelStateHelper>();
            Container.Bind<IGraphBuilder>().ToTransient<GraphBuilder>();
            Container.Bind<IShortestPathSolver>().ToTransient<ShortestPathSolver>();
            Container.Bind<ILineLoader>().ToTransient<LineLoader>();
            Container.Bind<ILineDrawer>().ToTransient<LineDrawer>();
            Container.Bind<ICellInformationProvider>().ToTransient<CellInformationProvider>();
            Container.Bind<IMazeStatsGenerator>().ToTransient<MazeStatsGenerator>();
            Container.Bind<IUiModeSwitcher>().ToSingle<UiModeSwitcher>();
            Container.Bind<IAgentFactory>().ToTransient<AgentFactory>();
            Container.Bind<IAgentOptionsProvider>().ToTransient<AgentOptionsProvider>();
            Container.Bind<ITimeRecorder>().ToTransient<TimeRecorder>();

            Container.Bind<ICameraManagement>().ToSingle<CameraManagementExtra>();
            Container.Bind<IGrowingTreeAlgorithm>().ToTransient<GrowingTreeAlgorithmLinkedList>();
            Container.Bind<IRecursiveBacktrackerAlgorithm>().ToTransient<BacktrackerAlgorithm>();

        }
    }
}
