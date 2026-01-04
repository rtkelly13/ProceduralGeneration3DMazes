using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Helper;

namespace ProceduralMaze.Tests;

[TestFixture]
public class EdgeCaseTests
{
	private ServiceContainer _services = null!;

	[SetUp]
	public void Setup()
	{
		_services = new ServiceContainer();
	}

	#region Small Maze Tests

	[Test]
	public void GenerateMaze_2x2x1_CreatesMinimalMaze()
	{
		// Arrange
		var settings = new MazeGenerationSettings
		{
			Algorithm = Algorithm.GrowingTreeAlgorithm,
			Size = new MazeSize { X = 2, Y = 2, Z = 1 },
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
		Assert.That(result.HeuristicsResults.TotalCells, Is.EqualTo(4));
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
	}

	[Test]
	public void GenerateMaze_1x10x1_CreatesLinearMaze()
	{
		// Arrange - single row maze
		var settings = new MazeGenerationSettings
		{
			Algorithm = Algorithm.GrowingTreeAlgorithm,
			Size = new MazeSize { X = 1, Y = 10, Z = 1 },
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
		Assert.That(result.HeuristicsResults.TotalCells, Is.EqualTo(10));
		// Linear maze should have path length of at most 9 (moving through all cells)
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.LessThanOrEqualTo(9));
	}

	[Test]
	public void GenerateMaze_10x1x1_CreatesLinearMaze()
	{
		// Arrange - single column maze
		var settings = new MazeGenerationSettings
		{
			Algorithm = Algorithm.GrowingTreeAlgorithm,
			Size = new MazeSize { X = 10, Y = 1, Z = 1 },
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
		Assert.That(result.HeuristicsResults.TotalCells, Is.EqualTo(10));
	}

	#endregion

	#region MazePoint Tests

	[Test]
	public void MazePoint_Equals_SameCoordinates_ReturnsTrue()
	{
		// Arrange
		var p1 = new MazePoint(3, 4, 5);
		var p2 = new MazePoint(3, 4, 5);

		// Assert
		Assert.That(p1.Equals(p2), Is.True);
	}

	[Test]
	public void MazePoint_Equals_DifferentCoordinates_ReturnsFalse()
	{
		// Arrange
		var p1 = new MazePoint(3, 4, 5);

		// Assert
		Assert.That(p1.Equals(new MazePoint(0, 4, 5)), Is.False);
		Assert.That(p1.Equals(new MazePoint(3, 0, 5)), Is.False);
		Assert.That(p1.Equals(new MazePoint(3, 4, 0)), Is.False);
	}

	[Test]
	public void MazePoint_Set_UpdatesCoordinates()
	{
		// Arrange
		var p = new MazePoint(1, 2, 3);

		// Act
		p.Set(4, 5, 6);

		// Assert
		Assert.That(p.X, Is.EqualTo(4));
		Assert.That(p.Y, Is.EqualTo(5));
		Assert.That(p.Z, Is.EqualTo(6));
	}

	[Test]
	public void MazePoint_GetHashCode_SameForEqualPoints()
	{
		// Arrange
		var p1 = new MazePoint(3, 4, 5);
		var p2 = new MazePoint(3, 4, 5);

		// Assert
		Assert.That(p1.GetHashCode(), Is.EqualTo(p2.GetHashCode()));
	}

	#endregion

	#region DoorsAtEdge Tests

	[Test]
	public void GenerateMaze_DoorsAtEdge_True_StartEndAtEdge()
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
			GrowingTreeSettings = new GrowingTreeSettings()
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var start = result.MazeJumper.StartPoint;
		var end = result.MazeJumper.EndPoint;
		var size = settings.Size;

		// Assert - both start and end should be at an edge (X, Y, or Z boundary)
		bool startAtEdge = start.X == 0 || start.X == size.X - 1 || 
		                   start.Y == 0 || start.Y == size.Y - 1 ||
		                   start.Z == 0 || start.Z == size.Z - 1;
		bool endAtEdge = end.X == 0 || end.X == size.X - 1 || 
		                 end.Y == 0 || end.Y == size.Y - 1 ||
		                 end.Z == 0 || end.Z == size.Z - 1;
		Assert.That(startAtEdge, Is.True, 
			"With DoorsAtEdge=true, start should be at edge");
		Assert.That(endAtEdge, Is.True, 
			"With DoorsAtEdge=true, end should be at edge");
	}

	[Test]
	public void GenerateMaze_DoorsAtEdge_False_StillGeneratesValidMaze()
	{
		// Arrange
		var settings = new MazeGenerationSettings
		{
			Algorithm = Algorithm.GrowingTreeAlgorithm,
			Size = new MazeSize { X = 10, Y = 10, Z = 1 },
			Option = MazeType.ArrayBidirectional,
			DoorsAtEdge = false,
			WallRemovalPercent = 0,
			AgentType = AgentType.None,
			GrowingTreeSettings = new GrowingTreeSettings()
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert - maze is still valid
		Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
	}

	#endregion

	#region Agent Integration Tests

	[Test]
	public void GenerateMaze_WithPerfectAgent_ReturnsAgentResults()
	{
		// Arrange
		var settings = new MazeGenerationSettings
		{
			Algorithm = Algorithm.GrowingTreeAlgorithm,
			Size = new MazeSize { X = 5, Y = 5, Z = 1 },
			Option = MazeType.ArrayBidirectional,
			DoorsAtEdge = true,
			WallRemovalPercent = 0,
			AgentType = AgentType.Perfect,
			GrowingTreeSettings = new GrowingTreeSettings()
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result.AgentResults, Is.Not.Null);
		Assert.That(result.AgentResults!.Movements.Count, Is.GreaterThan(0));
	}

	[Test]
	public void GenerateMaze_WithRandomAgent_ReturnsAgentResults()
	{
		// Arrange
		var settings = new MazeGenerationSettings
		{
			Algorithm = Algorithm.GrowingTreeAlgorithm,
			Size = new MazeSize { X = 5, Y = 5, Z = 1 },
			Option = MazeType.ArrayBidirectional,
			DoorsAtEdge = true,
			WallRemovalPercent = 0,
			AgentType = AgentType.Random,
			GrowingTreeSettings = new GrowingTreeSettings()
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert
		Assert.That(result.AgentResults, Is.Not.Null);
		Assert.That(result.AgentResults!.Movements.Count, Is.GreaterThan(0));
	}

	#endregion

	#region Timing/Recording Tests

	[Test]
	public void GenerateMaze_RecordsTimings()
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
			GrowingTreeSettings = new GrowingTreeSettings()
		};

		// Act
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);

		// Assert - timings should be recorded
		Assert.That(result.TotalTime, Is.GreaterThan(TimeSpan.Zero));
	}

	#endregion
}
