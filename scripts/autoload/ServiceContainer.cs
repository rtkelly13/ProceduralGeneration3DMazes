using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Heuristics;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Serialization;
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.Autoload
{
    /// <summary>
    /// Simple service container to replace Zenject DI.
    /// Creates and wires up all dependencies manually.
    /// </summary>
    public class ServiceContainer
    {
        // Helper classes (stateless, can be shared)
        public IDirectionsFlagParser DirectionsFlagParser { get; }
        public IMovementHelper MovementHelper { get; }
        public IPointValidity PointValidity { get; }
        public IRandomValueGenerator RandomValueGenerator { get; }
        public ITimeRecorder TimeRecorder { get; }
        public IMazeHelper MazeHelper { get; }

        // Model classes
        public IMazeArrayBuilder MazeArrayBuilder { get; }
        public IMazePointFactory MazePointFactory { get; }

        // Generation classes
        public IRandomPointGenerator RandomPointGenerator { get; }
        public IPointsAndDirectionsRetriever PointsAndDirectionsRetriever { get; }
        public IDeadEndFiller DeadEndFiller { get; }
        public IRandomCarver RandomCarver { get; }
        public IWallRemovalCalculator WallRemovalCalculator { get; }

        // Algorithms
        public IGrowingTreeAlgorithm GrowingTreeAlgorithm { get; }
        public IRecursiveBacktrackerAlgorithm RecursiveBacktrackerAlgorithm { get; }
        public IBinaryTreeAlgorithm BinaryTreeAlgorithm { get; }
        public IPrimsAlgorithm PrimsAlgorithm { get; }

        // Solver classes
        public IGraphBuilder GraphBuilder { get; }
        public ISolverFactory SolverFactory { get; }
        public IShortestPathSolver ShortestPathSolver { get; }
        public IKShortestPathsSolver KShortestPathsSolver { get; }
        public DijkstraAnimator DijkstraAnimator { get; }

        // Agent classes
        public IAgentFactory AgentFactory { get; }

        // Heuristics classes
        public IMazeStatsGenerator MazeStatsGenerator { get; }
        public IHeuristicsGenerator HeuristicsGenerator { get; }

        // Factory classes
        public IModelsWrapperFactory ModelsWrapperFactory { get; }
        public IDeadEndModelWrapperFactory DeadEndModelWrapperFactory { get; }
        public IMazeFactory MazeFactory { get; }
        public IMazeModelFactory MazeModelFactory { get; }
        public IMazeGenerationFactory MazeGenerationFactory { get; }

        // Serialization classes
        public IMazeSerializer MazeSerializer { get; }
        public IMazeDeserializer MazeDeserializer { get; }
        public IMazeValidator MazeValidator { get; }
        public IMazeStatsSerializer MazeStatsSerializer { get; }

        public ServiceContainer()
        {
            // Helper classes (order matters - some depend on others)
            DirectionsFlagParser = new DirectionsFlagParser();
            PointValidity = new PointValidity();
            RandomValueGenerator = new RandomValueGenerator();
            TimeRecorder = new TimeRecorder();
            
            // Model classes
            MazePointFactory = new MazePointFactory();
            MazeArrayBuilder = new MazeArrayBuilder(MazePointFactory);
            
            // MazeHelper depends on MazePointFactory
            MazeHelper = new MazeHelper(MazePointFactory);
            
            // MovementHelper depends on DirectionsFlagParser, MazePointFactory, PointValidity
            MovementHelper = new MovementHelper(DirectionsFlagParser, MazePointFactory, PointValidity);

            // Generation classes
            RandomPointGenerator = new RandomPointGenerator(RandomValueGenerator, MazePointFactory);
            PointsAndDirectionsRetriever = new PointsAndDirectionsRetriever(MazeHelper);
            WallRemovalCalculator = new WallRemovalCalculator();

            // Factory classes (needed by DeadEndFiller and others)
            ModelsWrapperFactory = new ModelsWrapperFactory();
            DeadEndModelWrapperFactory = new DeadEndModelWrapperFactory(MazeArrayBuilder, DirectionsFlagParser, MovementHelper);
            
            // More generation classes
            DeadEndFiller = new DeadEndFiller(DeadEndModelWrapperFactory, PointsAndDirectionsRetriever);
            RandomCarver = new RandomCarver(RandomPointGenerator, PointsAndDirectionsRetriever, DirectionsFlagParser);

            // Algorithms
            GrowingTreeAlgorithm = new GrowingTreeAlgorithmLinkedList(RandomPointGenerator, RandomValueGenerator, DirectionsFlagParser);
            RecursiveBacktrackerAlgorithm = new BacktrackerAlgorithm(DirectionsFlagParser, RandomPointGenerator);
            BinaryTreeAlgorithm = new BinaryTreeAlgorithm(DirectionsFlagParser, RandomPointGenerator);
            PrimsAlgorithm = new PrimsAlgorithm(DirectionsFlagParser, RandomPointGenerator, RandomValueGenerator);

            // Solver classes
            GraphBuilder = new GraphBuilder(PointsAndDirectionsRetriever, DirectionsFlagParser);
            SolverFactory = new SolverFactory(GraphBuilder);
            ShortestPathSolver = new ShortestPathSolver(GraphBuilder);
            KShortestPathsSolver = new KShortestPathsSolver(GraphBuilder);
            DijkstraAnimator = new DijkstraAnimator(GraphBuilder);

            // Agent classes
            AgentFactory = new AgentFactory(DirectionsFlagParser, PointsAndDirectionsRetriever);

            // Heuristics classes
            MazeStatsGenerator = new MazeStatsGenerator(DirectionsFlagParser);
            HeuristicsGenerator = new HeuristicsGenerator(SolverFactory, MazeStatsGenerator);

            // Remaining factory classes
            MazeFactory = new MazeFactory(PointValidity, MovementHelper, DirectionsFlagParser, RandomPointGenerator, ModelsWrapperFactory, DeadEndModelWrapperFactory, PointsAndDirectionsRetriever);
            MazeModelFactory = new MazeModelFactory(MovementHelper, DirectionsFlagParser, MazePointFactory, MazeArrayBuilder, RandomPointGenerator);
            MazeGenerationFactory = new MazeGenerationFactory(
                MazeModelFactory,
                GrowingTreeAlgorithm,
                MazeFactory,
                DeadEndFiller,
                RandomCarver,
                WallRemovalCalculator,
                RecursiveBacktrackerAlgorithm,
                BinaryTreeAlgorithm,
                PrimsAlgorithm,
                HeuristicsGenerator,
                AgentFactory,
                TimeRecorder,
                MazeHelper);

            // Serialization classes
            MazeSerializer = new MazeSerializer();
            MazeDeserializer = new MazeDeserializer();
            MazeValidator = new MazeValidator(DirectionsFlagParser, MovementHelper);
            MazeStatsSerializer = new MazeStatsSerializer();
        }
    }
}
