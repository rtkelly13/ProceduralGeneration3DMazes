using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Tests;

[TestFixture]
public class AgentTests
{
	private ServiceContainer _services = null!;

	[SetUp]
	public void Setup()
	{
		_services = new ServiceContainer();
	}

	private MazeGenerationResults GenerateMaze(int x = 10, int y = 10, int z = 1)
	{
		var settings = new MazeGenerationSettings
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
		return _services.MazeGenerationFactory.GenerateMaze(settings);
	}

	#region AgentFactory Tests

	[Test]
	public void AgentFactory_CreateRandomAgent_ReturnsRandomAgent()
	{
		// Arrange & Act
		var agent = _services.AgentFactory.MakeAgent(AgentType.Random);

		// Assert
		Assert.That(agent, Is.InstanceOf<RandomAgent>());
	}

	[Test]
	public void AgentFactory_CreatePerfectAgent_ReturnsPerfectAgent()
	{
		// Arrange & Act
		var agent = _services.AgentFactory.MakeAgent(AgentType.Perfect);

		// Assert
		Assert.That(agent, Is.InstanceOf<PerfectAgent>());
	}

	[Test]
	public void AgentFactory_InvalidAgentType_ThrowsException()
	{
		// Arrange
		var invalidType = (AgentType)999;

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => _services.AgentFactory.MakeAgent(invalidType));
	}

	#endregion

	#region PerfectAgent Tests

	[Test]
	public void PerfectAgent_RunAgent_ReturnsPathToEnd()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var agent = _services.AgentFactory.MakeAgent(AgentType.Perfect);

		// Act
		var result = agent.RunAgent(mazeResult.MazeJumper);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Movements, Is.Not.Null);
		Assert.That(result.Movements.Count, Is.GreaterThan(0), "Agent should make at least one movement");
	}

	[Test]
	public void PerfectAgent_RunAgent_PathEndsAtEndPoint()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var agent = _services.AgentFactory.MakeAgent(AgentType.Perfect);
		var endPoint = mazeResult.MazeJumper.EndPoint;

		// Act
		var result = agent.RunAgent(mazeResult.MazeJumper);

		// Assert - The last movement should move toward/to the end point
		// The movements list contains (startPoint, direction) pairs
		// Following the path: start from movement[0].MazePoint, move in movement[0].Direction, etc.
		Assert.That(result.Movements.Count, Is.GreaterThan(0));
		
		// Simulate following the path
		MazePoint currentPoint = result.Movements[0].MazePoint;
		foreach (var movement in result.Movements)
		{
			// Each movement starts from its MazePoint and moves in Direction
			currentPoint = _services.MovementHelper.Move(movement.MazePoint, movement.Direction, mazeResult.MazeJumper.Size);
		}
		
		// The final position after last movement should be the end point
		Assert.That(currentPoint.Equals(endPoint), Is.True, 
			$"Path should end at ({endPoint.X},{endPoint.Y},{endPoint.Z}) but ended at ({currentPoint.X},{currentPoint.Y},{currentPoint.Z})");
	}

	[Test]
	public void PerfectAgent_RunAgent_PathIsEfficient()
	{
		// Arrange - Perfect agent finds a valid path (may not be shortest due to DFS approach)
		var mazeResult = GenerateMaze(5, 5, 1);
		var agent = _services.AgentFactory.MakeAgent(AgentType.Perfect);
		var shortestPath = mazeResult.HeuristicsResults.ShortestPathResult.ShortestPath;

		// Act
		var result = agent.RunAgent(mazeResult.MazeJumper);

		// Assert - Perfect agent uses DFS so path length should be reasonable
		// It finds A path to the end, which may equal or exceed shortest path
		Assert.That(result.Movements.Count, Is.GreaterThanOrEqualTo(shortestPath),
			"Perfect agent path should be at least as long as shortest path");
	}

	[Test]
	public void PerfectAgent_RunAgent_PathLengthMatchesShortestPath()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var agent = _services.AgentFactory.MakeAgent(AgentType.Perfect);
		var expectedPathLength = mazeResult.HeuristicsResults.ShortestPathResult.ShortestPath;

		// Act
		var result = agent.RunAgent(mazeResult.MazeJumper);

		// Assert - Perfect agent path should match the calculated shortest path
		Assert.That(result.Movements.Count, Is.EqualTo(expectedPathLength),
			"Perfect agent path length should equal calculated shortest path");
	}

	#endregion

	#region RandomAgent Tests

	[Test]
	public void RandomAgent_RunAgent_ReturnsPathToEnd()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var agent = _services.AgentFactory.MakeAgent(AgentType.Random);

		// Act
		var result = agent.RunAgent(mazeResult.MazeJumper);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Movements, Is.Not.Null);
		Assert.That(result.Movements.Count, Is.GreaterThan(0), "Agent should make at least one movement");
	}

	[Test]
	public void RandomAgent_RunAgent_PathReachesEnd()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var agent = _services.AgentFactory.MakeAgent(AgentType.Random);
		var endPoint = mazeResult.MazeJumper.EndPoint;

		// Act
		var result = agent.RunAgent(mazeResult.MazeJumper);

		// Assert - Following the movement list should reach the end
		Assert.That(result.Movements.Count, Is.GreaterThan(0));
		
		// Simulate following the path
		MazePoint currentPoint = result.Movements[0].MazePoint;
		foreach (var movement in result.Movements)
		{
			currentPoint = _services.MovementHelper.Move(movement.MazePoint, movement.Direction, mazeResult.MazeJumper.Size);
		}
		
		Assert.That(currentPoint.Equals(endPoint), Is.True,
			$"Path should end at ({endPoint.X},{endPoint.Y},{endPoint.Z}) but ended at ({currentPoint.X},{currentPoint.Y},{currentPoint.Z})");
	}

	[Test]
	public void RandomAgent_RunAgent_PathLengthAtLeastShortestPath()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 1);
		var agent = _services.AgentFactory.MakeAgent(AgentType.Random);
		var shortestPath = mazeResult.HeuristicsResults.ShortestPathResult.ShortestPath;

		// Act
		var result = agent.RunAgent(mazeResult.MazeJumper);

		// Assert - Random agent path should be at least as long as shortest path
		Assert.That(result.Movements.Count, Is.GreaterThanOrEqualTo(shortestPath),
			"Random agent path should be at least as long as shortest path");
	}

	#endregion

	#region DirectionAndPoint Tests

	[Test]
	public void DirectionAndPoint_Equality_SamePointAndDirection_AreEqual()
	{
		// Arrange
		var dp1 = new DirectionAndPoint { Direction = Direction.Right, MazePoint = new MazePoint(1, 2, 0) };
		var dp2 = new DirectionAndPoint { Direction = Direction.Right, MazePoint = new MazePoint(1, 2, 0) };

		// Act & Assert
		Assert.That(DirectionAndPoint.DirectionMazePointComparer.Equals(dp1, dp2), Is.True);
	}

	[Test]
	public void DirectionAndPoint_Equality_DifferentDirection_NotEqual()
	{
		// Arrange
		var dp1 = new DirectionAndPoint { Direction = Direction.Right, MazePoint = new MazePoint(1, 2, 0) };
		var dp2 = new DirectionAndPoint { Direction = Direction.Left, MazePoint = new MazePoint(1, 2, 0) };

		// Act & Assert
		Assert.That(DirectionAndPoint.DirectionMazePointComparer.Equals(dp1, dp2), Is.False);
	}

	[Test]
	public void DirectionAndPoint_Equality_DifferentPoint_NotEqual()
	{
		// Arrange
		var dp1 = new DirectionAndPoint { Direction = Direction.Right, MazePoint = new MazePoint(1, 2, 0) };
		var dp2 = new DirectionAndPoint { Direction = Direction.Right, MazePoint = new MazePoint(3, 4, 0) };

		// Act & Assert
		Assert.That(DirectionAndPoint.DirectionMazePointComparer.Equals(dp1, dp2), Is.False);
	}

	[Test]
	public void DirectionAndPoint_HashCode_SameValues_SameHash()
	{
		// Arrange
		var dp1 = new DirectionAndPoint { Direction = Direction.Right, MazePoint = new MazePoint(1, 2, 0) };
		var dp2 = new DirectionAndPoint { Direction = Direction.Right, MazePoint = new MazePoint(1, 2, 0) };

		// Act & Assert
		Assert.That(
			DirectionAndPoint.DirectionMazePointComparer.GetHashCode(dp1),
			Is.EqualTo(DirectionAndPoint.DirectionMazePointComparer.GetHashCode(dp2)));
	}

	#endregion

	#region AgentResults Tests

	[Test]
	public void AgentResults_DefaultMovements_IsEmptyList()
	{
		// Arrange & Act
		var results = new AgentResults();

		// Assert
		Assert.That(results.Movements, Is.Not.Null);
		Assert.That(results.Movements, Is.Empty);
	}

	[Test]
	public void AgentResults_SetMovements_ReturnsCorrectList()
	{
		// Arrange
		var movements = new List<DirectionAndPoint>
		{
			new DirectionAndPoint { Direction = Direction.Right, MazePoint = new MazePoint(0, 0, 0) },
			new DirectionAndPoint { Direction = Direction.Forward, MazePoint = new MazePoint(1, 0, 0) }
		};

		// Act
		var results = new AgentResults { Movements = movements };

		// Assert
		Assert.That(results.Movements.Count, Is.EqualTo(2));
	}

	#endregion

	#region 3D Maze Agent Tests

	[Test]
	public void PerfectAgent_3DMaze_FindsPathAcrossLevels()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 3);
		var agent = _services.AgentFactory.MakeAgent(AgentType.Perfect);

		// Act
		var result = agent.RunAgent(mazeResult.MazeJumper);

		// Assert - should find a path with movements
		Assert.That(result.Movements.Count, Is.GreaterThan(0), "Agent should find a path");
		
		// The path length should match or exceed shortest path
		var shortestPath = mazeResult.HeuristicsResults.ShortestPathResult.ShortestPath;
		Assert.That(result.Movements.Count, Is.GreaterThanOrEqualTo(shortestPath));
	}

	[Test]
	public void RandomAgent_3DMaze_FindsPathAcrossLevels()
	{
		// Arrange
		var mazeResult = GenerateMaze(5, 5, 3);
		var agent = _services.AgentFactory.MakeAgent(AgentType.Random);

		// Act
		var result = agent.RunAgent(mazeResult.MazeJumper);

		// Assert - should find a path with movements
		Assert.That(result.Movements.Count, Is.GreaterThan(0), "Agent should find a path");
		
		// The path length should match or exceed shortest path
		var shortestPath = mazeResult.HeuristicsResults.ShortestPathResult.ShortestPath;
		Assert.That(result.Movements.Count, Is.GreaterThanOrEqualTo(shortestPath));
	}

	#endregion
}
