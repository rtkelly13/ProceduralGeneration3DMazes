using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Ninject.Modules;

namespace ProcGenMaze.Experiments
{
    public class Container: NinjectModule
    {
        public override void Load()
        {
            Bind<IMovementHelper>().To<MovementHelper>();
            Bind<IDirectionsFlagParser>().To<DirectionsFlagParser>();
            Bind<IMazePointFactory>().To<MazePointFactory>();
            Bind<IPointValidity>().To<PointValidity>();
            Bind<IRandomPointGenerator>().To<RandomPointGenerator>();
            Bind<IRandomValueGenerator>().To<RandomValueGenerator>();
            Bind<ILoadCellPrefab>().To<LoadCellPrefab>();
            Bind<ISpriteCache>().To<SpriteCache>().InSingletonScope();
            Bind<ICellInformationLoader>().To<CellInformationLoader>();
            Bind<ICurrentSettingsHolder>().To<CurrentSettingsHolder>();
            Bind<IMazeUiBuilder>().To<MazeUiBuilder>();
            Bind<IInputHandler>().To<InputHandler>();
            Bind<IMazeHelper>().To<MazeHelper>();
            Bind<ICurrentMazeHolder>().To<CurrentMazeHolder>();

            Bind<IAlgorithmsProvider>().To<AlgorithmsProvider>().InSingletonScope(); ;
            Bind<IResourceLoader>().To<ResourceLoader>();
            Bind<ISettingBuilder>().To<GrowingTreeSettingsBuilder>().InSingletonScope(); ;
            Bind<ISettingBuilder>().To<NoneSettingsBuilder>();
            Bind<ISettingBuilder>().To<RecursiveBacktrackerSettingsBuilder>().InSingletonScope(); ;
            Bind<ISettingBuilder>().To<BinaryTreeSettingsBuilder>().InSingletonScope(); ;
            Bind<ISettingsBuilder>().To<SettingsBuilder>().InSingletonScope(); ;
            Bind<IBaseValidateSettings>().To<BaseValidateSettings>();
            Bind<IValidateSetting>().To<RecursiveBacktrackerValidator>();
            Bind<IValidateSetting>().To<GrowingTreeAlgorithmValidator>();
            Bind<IValidateSetting>().To<NoneAlgorithmValidator>();
            Bind<IValidateSetting>().To<BinaryTreeValidator>();
            Bind<IAlgorithmSettingsInitialiser>().To<AlgorithmSettingsInitialiser>();
            Bind<IValidationRetriever>().To<ValidationRetriever>();
            Bind<IValidateSettings>().To<ValidateSettings>();
            Bind<IGrowingTreeStrategyStorage>().To<GrowingTreeStrategyStorage>();
            Bind<IModelOptionsProvider>().To<ModelOptionsProvider>();
            Bind<IYesNoOptionsProvider>().To<YesNoOptionsProvider>();
            Bind<IWallCarverOptionsProvider>().To<WallCarverOptionsProvider>();
            Bind<IPointsAndDirectionsRetriever>().To<PointsAndDirectionsRetriever>();

            Bind<ISceneLoader>().To<SceneLoader>();
            Bind<IMazeGenerationFactory>().To<MazeGenerationFactory>();
            Bind<IMazeModelFactory>().To<MazeModelFactory>();
            Bind<IBinaryTreeAlgorithm>().To<BinaryTreeAlgorithm>();
            Bind<IMazeFactory>().To<MazeFactory>();
            Bind<IMazeValidator>().To<MazeValidator>();
            Bind<IMazeArrayBuilder>().To<MazeArrayBuilder>();
            Bind<IDeadEndModelWrapperFactory>().To<DeadEndModelWrapperFactory>();
            Bind<IDeadEndFiller>().To<DeadEndFiller>();
            Bind<IModelsWrapperFactory>().To<ModelsWrapperFactory>();
            Bind<IRandomCarver>().To<RandomCarver>();
            Bind<IExtraWallCalculator>().To<ExtraWallCalculator>();
            Bind<IGenerateTestCase>().To<GenerateTestCase>();
            Bind<IDoorwayLoader>().To<DoorwayLoader>();
            Bind<ICircleLoader>().To<CircleLoader>();
            Bind<IMazeNeedsGenerating>().To<MazeNeedsGenerating>();
            Bind<IHeuristicsGenerator>().To<HeuristicsGenerator>();
            Bind<IModelStateHelper>().To<ModelStateHelper>();
            Bind<IGraphBuilder>().To<GraphBuilder>();
            Bind<IShortestPathSolver>().To<ShortestPathSolver>();
            Bind<ILineLoader>().To<LineLoader>();
            Bind<ILineDrawer>().To<LineDrawer>();
            Bind<ICellInformationProvider>().To<CellInformationProvider>();
            Bind<IMazeStatsGenerator>().To<MazeStatsGenerator>();
            Bind<IUiModeSwitcher>().To<UiModeSwitcher>().InSingletonScope(); ;
            Bind<IAgentFactory>().To<AgentFactory>();
            Bind<IAgentOptionsProvider>().To<AgentOptionsProvider>();
            Bind<ITimeRecorder>().To<TimeRecorder>();

            Bind<ICameraManagement>().To<CameraManagementExtra>().InSingletonScope(); ;
            Bind<IGrowingTreeAlgorithm>().To<GrowingTreeAlgorithmLinkedList>();
            Bind<IRecursiveBacktrackerAlgorithm>().To<BacktrackerAlgorithm>();
            Bind<IArrayHelper>().To<ArrayHelper>();
            Bind<IExperimentRunner>().To<ExperimentRunner>();
            Bind<IOutputWriter>().To<OutputWriter>();
        }
    }
}
