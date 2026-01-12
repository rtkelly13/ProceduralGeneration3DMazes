using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;
using ProceduralMaze.Maze.Solver.Heuristics;

namespace ProceduralMaze.Tests;

[TestFixture]
public class AStarSolverTests
{
    private ServiceContainer _services = null!;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceContainer();
    }

    [Test]
    public void AStarSolver_FindsSamePathLengthAsDijkstra_2D()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 5, Y = 5, Z = 1 },
            Option = MazeType.ArrayBidirectional,
            GrowingTreeSettings = new GrowingTreeSettings { NewestWeight = 100 }
        };
        var generationResult = _services.MazeGenerationFactory.GenerateMaze(settings);
        var jumper = generationResult.MazeJumper;
        
        // Dijkstra (existing)
        var dijkstra = _services.ShortestPathSolver;
        var dijkstraResult = dijkstra.GetGraph(jumper);

        // A* (Manhattan)
        var heuristic = new ManhattanHeuristic();
        var astar = new AStarSolver(_services.GraphBuilder, heuristic);
        var astarResult = astar.GetGraph(jumper);

        // Assert
        Assert.That(astarResult.ShortestPath, Is.EqualTo(dijkstraResult.ShortestPath));
    }

    [Test]
    public void AStarSolver_FindsSamePathLengthAsDijkstra_3D()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 5, Y = 5, Z = 5 },
            Option = MazeType.ArrayBidirectional,
            GrowingTreeSettings = new GrowingTreeSettings { NewestWeight = 100 }
        };
        var generationResult = _services.MazeGenerationFactory.GenerateMaze(settings);
        var jumper = generationResult.MazeJumper;
        
        // Dijkstra (existing)
        var dijkstra = _services.ShortestPathSolver;
        var dijkstraResult = dijkstra.GetGraph(jumper);

        // A* (Euclidean)
        var heuristic = new EuclideanHeuristic();
        var astar = new AStarSolver(_services.GraphBuilder, heuristic);
        var astarResult = astar.GetGraph(jumper);

        // Assert
        Assert.That(astarResult.ShortestPath, Is.EqualTo(dijkstraResult.ShortestPath));
    }
}
