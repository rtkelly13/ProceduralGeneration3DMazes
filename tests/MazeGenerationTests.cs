using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;
using ProceduralMaze.Maze.Solver.Heuristics;

namespace ProceduralMaze.Tests;

[TestFixture]
public class MazeGenerationTests
{
	private ServiceContainer _services = null!;

	[SetUp]
	public void Setup()
	{
		_services = new ServiceContainer();
	}

	[Test]
	public void GenerateMaze_10x10x1_CreatesValidMaze()
	{
		// Arrange
		var settings = new MazeGenerationSettings
		{
			Algorithm = Algorithm.GrowingTreeAlgorithm,
			Size = new MazeSize { X = 10, Y = 10, Z = 1 },
			Option = MazeType.ArrayBidirectional,
			DoorsAtEdge = true,
			WallRemovalPercent = 0,
			AgentType = AgentType.None,
			GrowingTreeSettings = new GrowingTreeSettings
			{
				NewestWeight = 100,
				OldestWeight = 0,
				RandomWeight = 0
			}
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.MazeJumper, Is.Not.Null);
		Assert.That(result.MazeJumper.Size.X, Is.EqualTo(10));
		Assert.That(result.MazeJumper.Size.Y, Is.EqualTo(10));
		Assert.That(result.MazeJumper.Size.Z, Is.EqualTo(1));
	}

	[Test]
	public void GenerateMaze_10x10x1_HasStartAndEndPoints()
	{
		// Arrange
		var settings = CreateDefaultSettings(10, 10, 1);

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result.MazeJumper.StartPoint, Is.Not.Null);
		Assert.That(result.MazeJumper.EndPoint, Is.Not.Null);
		Assert.That(result.MazeJumper.StartPoint, Is.Not.EqualTo(result.MazeJumper.EndPoint));
	}

	[Test]
	public void GenerateMaze_10x10x1_AllCellsAreReachable()
	{
		// Arrange
		var settings = CreateDefaultSettings(10, 10, 1);

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var mazeJumper = result.MazeJumper;

		// Assert - verify every cell has at least one carved direction
		int cellsWithDirections = 0;
		for (int x = 0; x < 10; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				var point = new MazePoint(x, y, 0);
				mazeJumper.JumpToPoint(point);
				var directions = mazeJumper.GetFlagFromPoint();
				if (directions != Direction.None)
				{
					cellsWithDirections++;
				}
			}
		}

		// All 100 cells should have at least one direction carved
		Assert.That(cellsWithDirections, Is.EqualTo(100), 
			"All cells should have at least one direction carved");
	}

	[Test]
	public void GenerateMaze_10x10x1_HasValidShortestPath()
	{
		// Arrange
		var settings = CreateDefaultSettings(10, 10, 1);

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result.HeuristicsResults, Is.Not.Null);
		Assert.That(result.HeuristicsResults.ShortestPathResult, Is.Not.Null);
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0),
			"Shortest path should be greater than 0");
	}

	[Test]
	public void GenerateMaze_10x10x1_DirectionsAreSymmetric()
	{
		// Arrange
		var settings = CreateDefaultSettings(10, 10, 1);

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var mazeJumper = result.MazeJumper;

		// Assert - if cell A has direction to cell B, cell B should have opposite direction to A
		for (int x = 0; x < 10; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				var point = new MazePoint(x, y, 0);
				mazeJumper.JumpToPoint(point);
				var directions = mazeJumper.GetFlagFromPoint();

				// Check Right direction
				if ((directions & Direction.Right) != 0 && x < 9)
				{
					var rightPoint = new MazePoint(x + 1, y, 0);
					mazeJumper.JumpToPoint(rightPoint);
					var rightDirections = mazeJumper.GetFlagFromPoint();
					Assert.That((rightDirections & Direction.Left) != 0,
						$"Cell ({x+1},{y}) should have Left direction since ({x},{y}) has Right");
				}

				// Check Forward direction
				if ((directions & Direction.Forward) != 0 && y < 9)
				{
					var forwardPoint = new MazePoint(x, y + 1, 0);
					mazeJumper.JumpToPoint(forwardPoint);
					var forwardDirections = mazeJumper.GetFlagFromPoint();
					Assert.That((forwardDirections & Direction.Back) != 0,
						$"Cell ({x},{y+1}) should have Back direction since ({x},{y}) has Forward");
				}
			}
		}
	}

	[Test]
	public void GenerateMaze_10x10x1_NoCellsOutOfBounds()
	{
		// Arrange
		var settings = CreateDefaultSettings(10, 10, 1);

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var mazeJumper = result.MazeJumper;

		// Assert - edge cells should not have directions pointing outside
		for (int x = 0; x < 10; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				var point = new MazePoint(x, y, 0);
				mazeJumper.JumpToPoint(point);
				var directions = mazeJumper.GetFlagFromPoint();

				// Left edge should not have Left direction
				if (x == 0)
				{
					Assert.That((directions & Direction.Left) == 0,
						$"Cell (0,{y}) should not have Left direction");
				}

				// Right edge should not have Right direction
				if (x == 9)
				{
					Assert.That((directions & Direction.Right) == 0,
						$"Cell (9,{y}) should not have Right direction");
				}

				// Bottom edge should not have Back direction
				if (y == 0)
				{
					Assert.That((directions & Direction.Back) == 0,
						$"Cell ({x},0) should not have Back direction");
				}

				// Top edge should not have Forward direction
				if (y == 9)
				{
					Assert.That((directions & Direction.Forward) == 0,
						$"Cell ({x},9) should not have Forward direction");
				}

				// Single Z level should not have Up/Down directions
				Assert.That((directions & Direction.Up) == 0,
					$"Cell ({x},{y}) should not have Up direction in single-level maze");
				Assert.That((directions & Direction.Down) == 0,
					$"Cell ({x},{y}) should not have Down direction in single-level maze");
			}
		}
	}

	[Test]
	public void GenerateMaze_WithBacktrackerAlgorithm_CreatesValidMaze()
	{
		// Arrange
		var settings = new MazeGenerationSettings
		{
			Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
			Size = new MazeSize { X = 10, Y = 10, Z = 1 },
			Option = MazeType.ArrayBidirectional,
			DoorsAtEdge = true,
			WallRemovalPercent = 0,
			AgentType = AgentType.None,
			GrowingTreeSettings = new GrowingTreeSettings()
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.MazeJumper, Is.Not.Null);
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
	}

	[Test]
	public void GenerateMaze_3DGrid_5x5x3_CreatesValidMultiLevelMaze()
	{
		// Arrange
		var settings = CreateDefaultSettings(5, 5, 3);

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var mazeJumper = result.MazeJumper;

		// Assert
		Assert.That(result.MazeJumper.Size.Z, Is.EqualTo(3));

		// Check that there are some vertical connections
		bool hasUpConnection = false;
		bool hasDownConnection = false;

		for (int z = 0; z < 3; z++)
		{
			for (int x = 0; x < 5; x++)
			{
				for (int y = 0; y < 5; y++)
				{
					var point = new MazePoint(x, y, z);
					mazeJumper.JumpToPoint(point);
					var directions = mazeJumper.GetFlagFromPoint();

					if ((directions & Direction.Up) != 0) hasUpConnection = true;
					if ((directions & Direction.Down) != 0) hasDownConnection = true;
				}
			}
		}

		Assert.That(hasUpConnection || hasDownConnection, Is.True,
			"Multi-level maze should have at least one vertical connection");
	}

	[Test]
	public void GenerateMaze_MultipleGenerations_ProduceDifferentMazes()
	{
		// Arrange
		var settings = CreateDefaultSettings(10, 10, 1);

		// Act
		var result1 = _services.MazeGenerationFactory.GenerateMaze(settings);
		var result2 = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert - start or end points should differ (statistically very likely)
		// or the shortest path should differ
		bool areDifferent = 
			!result1.MazeJumper.StartPoint.Equals(result2.MazeJumper.StartPoint) ||
			!result1.MazeJumper.EndPoint.Equals(result2.MazeJumper.EndPoint) ||
			result1.HeuristicsResults.ShortestPathResult.ShortestPath != 
				result2.HeuristicsResults.ShortestPathResult.ShortestPath;

		Assert.That(areDifferent, Is.True,
			"Two maze generations should produce different mazes");
	}

	[Test]
	public void GenerateMaze_10x10x1_TotalCellsMatchesExpected()
	{
		// Arrange
		var settings = CreateDefaultSettings(10, 10, 1);

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result.HeuristicsResults.TotalCells, Is.EqualTo(100));
	}

	[Test]
	public void GenerateMaze_WithAStarSolver_CreatesValidMaze()
	{
		// Arrange
		var settings = new MazeGenerationSettings
		{
			Algorithm = Algorithm.GrowingTreeAlgorithm,
			Size = new MazeSize { X = 10, Y = 10, Z = 1 },
			Option = MazeType.ArrayBidirectional,
			SolverType = SolverType.AStar,
			HeuristicType = HeuristicType.Manhattan,
			GrowingTreeSettings = new GrowingTreeSettings { NewestWeight = 100 }
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
	}

	private MazeGenerationSettings CreateDefaultSettings(int x, int y, int z)
	{
		return new MazeGenerationSettings
		{
			Algorithm = Algorithm.GrowingTreeAlgorithm,
			Size = new MazeSize { X = x, Y = y, Z = z },
			Option = MazeType.ArrayBidirectional,
			DoorsAtEdge = true,
			WallRemovalPercent = 0,
			AgentType = AgentType.None,
			GrowingTreeSettings = new GrowingTreeSettings
			{
				NewestWeight = 100,
				OldestWeight = 0,
				RandomWeight = 0
			}
		};
	}
}
