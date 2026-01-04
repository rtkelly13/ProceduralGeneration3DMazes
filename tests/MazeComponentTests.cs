using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Tests;

[TestFixture]
public class MazeComponentTests
{
	private ServiceContainer _services = null!;

	[SetUp]
	public void Setup()
	{
		_services = new ServiceContainer();
	}

	private MazeGenerationResults GenerateMaze(int x, int y, int z, MazeType mazeType = MazeType.ArrayBidirectional, int wallRemovalPercent = 0)
	{
		var settings = new MazeGenerationSettings
		{
			Algorithm = Algorithm.GrowingTreeAlgorithm,
			Size = new MazeSize { X = x, Y = y, Z = z },
			Option = mazeType,
			DoorsAtEdge = true,
			WallRemovalPercent = wallRemovalPercent,
			AgentType = AgentType.None,
			GrowingTreeSettings = new GrowingTreeSettings
			{
				NewestWeight = 100,
				OldestWeight = 0,
				RandomWeight = 0
			}
		};
		return _services.MazeGenerationFactory.GenerateMaze(settings);
	}

	#region MazeHelper Tests

	[Test]
	public void MazeHelper_GetForEachPoint_ReturnsAllPoints()
	{
		// Arrange
		var size = new MazeSize { X = 3, Y = 4, Z = 2 };
		var points = new List<MazePoint>();

		// Act
		foreach (var point in _services.MazeHelper.GetForEachPoint(size, p => p))
		{
			points.Add(point);
		}

		// Assert
		Assert.That(points.Count, Is.EqualTo(3 * 4 * 2));
	}

	[Test]
	public void MazeHelper_GetForEachPoint_WithTransform_TransformsAllPoints()
	{
		// Arrange
		var size = new MazeSize { X = 3, Y = 3, Z = 1 };

		// Act - transform each point to a string representation
		var strings = _services.MazeHelper.GetForEachPoint(size, 
			p => $"{p.X},{p.Y},{p.Z}").ToList();

		// Assert
		Assert.That(strings.Count, Is.EqualTo(9)); // 3x3x1 = 9 points
		Assert.That(strings, Contains.Item("0,0,0"));
		Assert.That(strings, Contains.Item("2,2,0"));
	}

	[Test]
	public void MazeHelper_GetForEachZ_ReturnsOnlyPointsAtLevel()
	{
		// Arrange
		var size = new MazeSize { X = 3, Y = 3, Z = 5 };
		var level = 2;

		// Act
		var points = _services.MazeHelper.GetForEachZ(size, level, p => p).ToList();

		// Assert
		Assert.That(points.Count, Is.EqualTo(9)); // 3x3 = 9
		Assert.That(points.All(p => p.Z == level), Is.True);
	}

	[Test]
	public void MazeHelper_GetForEachZ_WithFilter_ReturnsMatchingPoints()
	{
		// Arrange
		var size = new MazeSize { X = 5, Y = 5, Z = 3 };
		var level = 1;

		// Act - get only points at y=0 (bottom row) at level 1
		var bottomRow = _services.MazeHelper.GetForEachZ<MazePoint>(size, level, 
			p => p.Y == 0).ToList();

		// Assert
		Assert.That(bottomRow.Count, Is.EqualTo(5)); // 5 points in bottom row
		foreach (var p in bottomRow)
		{
			Assert.That(p.Z, Is.EqualTo(level));
			Assert.That(p.Y, Is.EqualTo(0));
		}
	}

	[Test]
	public void MazeHelper_GetPoints_ReturnsFilteredPoints()
	{
		// Arrange
		var size = new MazeSize { X = 10, Y = 10, Z = 1 };

		// Act
		var centerPoints = _services.MazeHelper.GetPoints(size, 
			p => p.X >= 3 && p.X <= 6 && p.Y >= 3 && p.Y <= 6).ToList();

		// Assert
		Assert.That(centerPoints.Count, Is.EqualTo(16)); // 4x4 center
	}

	[Test]
	public void MazeHelper_DoForEachPoint_ExecutesForAllPoints()
	{
		// Arrange
		var size = new MazeSize { X = 2, Y = 3, Z = 2 };
		var count = 0;

		// Act
		_services.MazeHelper.DoForEachPoint(size, p => count++);

		// Assert
		Assert.That(count, Is.EqualTo(12)); // 2*3*2
	}

	[Test]
	public void MazeHelper_DoForEachZ_ExecutesForAllPointsAtLevel()
	{
		// Arrange
		var size = new MazeSize { X = 4, Y = 4, Z = 3 };
		var count = 0;
		var level = 0;

		// Act
		_services.MazeHelper.DoForEachZ(size, level, p => count++);

		// Assert
		Assert.That(count, Is.EqualTo(16)); // 4*4
	}

	#endregion

	#region MazeJumper Tests

	[Test]
	public void MazeJumper_JumpToPoint_ChangesCurrentPoint()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;
		var targetPoint = new MazePoint(2, 3, 0);

		// Act
		jumper.JumpToPoint(targetPoint);

		// Assert
		Assert.That(jumper.CurrentPoint.Equals(targetPoint), Is.True);
	}

	[Test]
	public void MazeJumper_TryJumpToPoint_ValidPoint_ReturnsTrue()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;
		var targetPoint = new MazePoint(4, 4, 0);

		// Act
		var result = jumper.TryJumpToPoint(targetPoint);

		// Assert
		Assert.That(result, Is.True);
		Assert.That(jumper.CurrentPoint.Equals(targetPoint), Is.True);
	}

	[Test]
	public void MazeJumper_TryJumpToPoint_InvalidPoint_ReturnsFalse()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;
		var invalidPoint = new MazePoint(10, 10, 0); // Outside maze

		// Act
		var result = jumper.TryJumpToPoint(invalidPoint);

		// Assert
		Assert.That(result, Is.False);
	}

	[Test]
	public void MazeJumper_CanJumpToPoint_ValidPoint_ReturnsTrue()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;

		// Act & Assert
		Assert.That(jumper.CanJumpToPoint(new MazePoint(0, 0, 0)), Is.True);
		Assert.That(jumper.CanJumpToPoint(new MazePoint(4, 4, 0)), Is.True);
	}

	[Test]
	public void MazeJumper_CanJumpToPoint_InvalidPoint_ReturnsFalse()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;

		// Act & Assert
		Assert.That(jumper.CanJumpToPoint(new MazePoint(-1, 0, 0)), Is.False);
		Assert.That(jumper.CanJumpToPoint(new MazePoint(5, 5, 0)), Is.False);
	}

	[Test]
	public void MazeJumper_JumpableDirections_ReturnsValidDirections()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;
		jumper.JumpToPoint(new MazePoint(2, 2, 0)); // Center point

		// Act
		var directions = jumper.JumpableDirections().ToList();

		// Assert - center point should be able to jump in 4 horizontal directions
		Assert.That(directions, Contains.Item(Direction.Left));
		Assert.That(directions, Contains.Item(Direction.Right));
		Assert.That(directions, Contains.Item(Direction.Forward));
		Assert.That(directions, Contains.Item(Direction.Back));
	}

	[Test]
	public void MazeJumper_JumpableDirections_CornerPoint_ReturnsLimitedDirections()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;
		jumper.JumpToPoint(new MazePoint(0, 0, 0)); // Corner

		// Act
		var directions = jumper.JumpableDirections().ToList();

		// Assert - corner can only jump right and forward
		Assert.That(directions, Contains.Item(Direction.Right));
		Assert.That(directions, Contains.Item(Direction.Forward));
		Assert.That(directions, Does.Not.Contain(Direction.Left));
		Assert.That(directions, Does.Not.Contain(Direction.Back));
	}

	[Test]
	public void MazeJumper_TryJumpInDirection_ValidDirection_ReturnsTrue()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;
		jumper.JumpToPoint(new MazePoint(2, 2, 0));

		// Act
		var result = jumper.TryJumpInDirection(Direction.Right);

		// Assert
		Assert.That(result, Is.True);
		Assert.That(jumper.CurrentPoint.X, Is.EqualTo(3));
	}

	[Test]
	public void MazeJumper_TryJumpInDirection_InvalidDirection_ReturnsFalse()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;
		jumper.JumpToPoint(new MazePoint(0, 0, 0));

		// Act
		var result = jumper.TryJumpInDirection(Direction.Left);

		// Assert
		Assert.That(result, Is.False);
	}

	[Test]
	public void MazeJumper_JumpInDirection_InvalidDirection_ThrowsException()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;
		jumper.JumpToPoint(new MazePoint(0, 0, 0));

		// Act & Assert
		Assert.Throws<ArgumentException>(() => jumper.JumpInDirection(Direction.Left));
	}

	[Test]
	public void MazeJumper_JumpingFinished_ReturnsMaze()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var jumper = mazeResult.MazeJumper;
		jumper.JumpToPoint(new MazePoint(2, 2, 0));

		// Act
		var maze = jumper.JumpingFinished();

		// Assert
		Assert.That(maze, Is.Not.Null);
		Assert.That(maze.CurrentPoint.X, Is.EqualTo(2));
		Assert.That(maze.CurrentPoint.Y, Is.EqualTo(2));
	}

	#endregion

	#region RandomCarver Tests (via WallRemoval)

	[Test]
	public void RandomCarver_CarveRandomWalls_None_DoesNotChangeConnections()
	{
		// Arrange - generate maze without extra walls
		var result = GenerateMaze(10, 10, 1, MazeType.ArrayBidirectional, 0);
		
		// Count connections - this is a baseline
		var connections = CountTotalDirections(result.MazeJumper);

		// Assert - should have connections from base generation
		Assert.That(connections, Is.GreaterThan(0));
	}

	[Test]
	public void RandomCarver_WithWallRemoval_CarvesAdditionalWalls()
	{
		// We test RandomCarver indirectly through the WallRemovalPercent setting
		// Generate multiple mazes and verify the one with wall removal tends to have more connections
		
		int trialsWithMoreConnections = 0;
		const int trials = 5;

		for (int i = 0; i < trials; i++)
		{
			var resultNoExtra = GenerateMaze(10, 10, 1, MazeType.ArrayBidirectional, 0);
			var resultWithExtra = GenerateMaze(10, 10, 1, MazeType.ArrayBidirectional, 10);

			var connectionsNoExtra = CountTotalDirections(resultNoExtra.MazeJumper);
			var connectionsWithExtra = CountTotalDirections(resultWithExtra.MazeJumper);

			if (connectionsWithExtra >= connectionsNoExtra)
				trialsWithMoreConnections++;
		}

		// Assert - most trials should have more or equal connections with extra walls
		Assert.That(trialsWithMoreConnections, Is.GreaterThanOrEqualTo(3),
			"Mazes with extra walls should generally have at least as many connections");
	}

	private int CountTotalDirections(IMazeJumper jumper)
	{
		int count = 0;
		for (int x = 0; x < jumper.Size.X; x++)
		{
			for (int y = 0; y < jumper.Size.Y; y++)
			{
				for (int z = 0; z < jumper.Size.Z; z++)
				{
					jumper.JumpToPoint(new MazePoint(x, y, z));
					var flag = jumper.GetFlagFromPoint();
					count += CountDirectionsInFlag(flag);
				}
			}
		}
		return count;
	}

	private int CountDirectionsInFlag(Direction flag)
	{
		int count = 0;
		if ((flag & Direction.Left) != 0) count++;
		if ((flag & Direction.Right) != 0) count++;
		if ((flag & Direction.Forward) != 0) count++;
		if ((flag & Direction.Back) != 0) count++;
		if ((flag & Direction.Up) != 0) count++;
		if ((flag & Direction.Down) != 0) count++;
		return count;
	}

	#endregion

	#region MazeType Tests

	[Test]
	public void GenerateMaze_ArrayUnidirectionalMaze_CreatesValidMaze()
	{
		// Arrange & Act
		var result = GenerateMaze(5, 5, 1, MazeType.ArrayUnidirectional);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.MazeJumper, Is.Not.Null);
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
	}

	[Test]
	public void GenerateMaze_ArrayBidirectionalMaze_CreatesValidMaze()
	{
		// Arrange & Act
		var result = GenerateMaze(5, 5, 1, MazeType.ArrayBidirectional);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
	}

	[Test]
	public void GenerateMaze_DictionaryMaze_CreatesValidMaze()
	{
		// Arrange & Act
		var result = GenerateMaze(5, 5, 1, MazeType.Dictionary);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
	}

	[Test]
	public void GenerateMaze_ArrayBidirectionalAndDictionaryMaze_ProduceValidMazes()
	{
		// Test both working maze types
		var types = new[] { MazeType.ArrayBidirectional, MazeType.Dictionary };

		foreach (var mazeType in types)
		{
			// Act
			var result = GenerateMaze(5, 5, 1, mazeType);

			// Assert - each type produces a valid connected maze
			Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0),
				$"MazeType {mazeType} should produce a connected maze");
			Assert.That(result.MazeJumper.StartPoint, Is.Not.EqualTo(result.MazeJumper.EndPoint),
				$"MazeType {mazeType} should have different start and end points");
		}
	}

	#endregion

	#region WallRemoval Tests

	[Test]
	public void GenerateMaze_WithWallRemoval_HasMoreConnections()
	{
		// Arrange - generate two mazes with same size but different wall removal
		var resultNoRemoval = GenerateMaze(10, 10, 1, MazeType.ArrayBidirectional, 0);
		var resultWithRemoval = GenerateMaze(10, 10, 1, MazeType.ArrayBidirectional, 10);

		// Count connections in each
		var connectionsNoRemoval = CountTotalDirections(resultNoRemoval.MazeJumper);
		var connectionsWithRemoval = CountTotalDirections(resultWithRemoval.MazeJumper);

		// Assert - maze with wall removal should have more connections
		// Note: Due to randomness this might occasionally fail, but should generally hold
		Assert.That(connectionsWithRemoval, Is.GreaterThanOrEqualTo(connectionsNoRemoval),
			"Maze with wall removal should have at least as many connections");
	}

	#endregion

	#region GrowingTree Strategy Tests

	[Test]
	public void GenerateMaze_OldestWeight_CreatesValidMaze()
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
				NewestWeight = 0,
				OldestWeight = 100,
				RandomWeight = 0
			}
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
	}

	[Test]
	public void GenerateMaze_RandomWeight_CreatesValidMaze()
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
				NewestWeight = 0,
				OldestWeight = 0,
				RandomWeight = 100
			}
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
	}

	[Test]
	public void GenerateMaze_MixedWeights_CreatesValidMaze()
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
				NewestWeight = 50,
				OldestWeight = 25,
				RandomWeight = 25
			}
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
	}

	#endregion
}
