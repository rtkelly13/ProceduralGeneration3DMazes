using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Tests;

[TestFixture]
public class MazeConnectivityTests
{
	private ServiceContainer _services = null!;

	[SetUp]
	public void Setup()
	{
		_services = new ServiceContainer();
	}

	[Test]
	public void GenerateMaze_10x10x1_IsFullyConnected()
	{
		// Arrange
		var settings = CreateDefaultSettings(10, 10, 1);
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var mazeJumper = result.MazeJumper;

		// Act - use flood fill from start point to verify all cells are reachable
		var visited = new HashSet<(int x, int y, int z)>();
		var queue = new Queue<(int x, int y, int z)>();
		
		var start = mazeJumper.StartPoint;
		queue.Enqueue((start.X, start.Y, start.Z));
		visited.Add((start.X, start.Y, start.Z));

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			var point = new MazePoint(current.x, current.y, current.z);
			mazeJumper.JumpToPoint(point);
			var directions = mazeJumper.GetFlagFromPoint();

			// Check each direction and add unvisited neighbors
			TryVisitNeighbor(directions, Direction.Left, current.x - 1, current.y, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Right, current.x + 1, current.y, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Back, current.x, current.y - 1, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Forward, current.x, current.y + 1, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Down, current.x, current.y, current.z - 1, visited, queue);
			TryVisitNeighbor(directions, Direction.Up, current.x, current.y, current.z + 1, visited, queue);
		}

		// Assert - all 100 cells should be reachable
		Assert.That(visited.Count, Is.EqualTo(100), 
			$"All 100 cells should be reachable from start, but only {visited.Count} were reached");
	}

	[Test]
	public void GenerateMaze_5x5x2_AllLevelsConnected()
	{
		// Arrange
		var settings = CreateDefaultSettings(5, 5, 2);
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var mazeJumper = result.MazeJumper;

		// Act - flood fill from start
		var visited = new HashSet<(int x, int y, int z)>();
		var queue = new Queue<(int x, int y, int z)>();
		
		var start = mazeJumper.StartPoint;
		queue.Enqueue((start.X, start.Y, start.Z));
		visited.Add((start.X, start.Y, start.Z));

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			var point = new MazePoint(current.x, current.y, current.z);
			mazeJumper.JumpToPoint(point);
			var directions = mazeJumper.GetFlagFromPoint();

			TryVisitNeighbor(directions, Direction.Left, current.x - 1, current.y, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Right, current.x + 1, current.y, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Back, current.x, current.y - 1, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Forward, current.x, current.y + 1, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Down, current.x, current.y, current.z - 1, visited, queue);
			TryVisitNeighbor(directions, Direction.Up, current.x, current.y, current.z + 1, visited, queue);
		}

		// Assert - all 50 cells (5x5x2) should be reachable
		Assert.That(visited.Count, Is.EqualTo(50), 
			$"All 50 cells should be reachable from start, but only {visited.Count} were reached");

		// Verify both levels have visited cells
		var level0Count = visited.Count(v => v.z == 0);
		var level1Count = visited.Count(v => v.z == 1);
		
		Assert.That(level0Count, Is.EqualTo(25), "Level 0 should have 25 reachable cells");
		Assert.That(level1Count, Is.EqualTo(25), "Level 1 should have 25 reachable cells");
	}

	[Test]
	public void GenerateMaze_EndPointIsReachableFromStart()
	{
		// Arrange
		var settings = CreateDefaultSettings(10, 10, 1);
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var mazeJumper = result.MazeJumper;

		// Act - flood fill from start to find end
		var visited = new HashSet<(int x, int y, int z)>();
		var queue = new Queue<(int x, int y, int z)>();
		
		var start = mazeJumper.StartPoint;
		var end = mazeJumper.EndPoint;
		
		queue.Enqueue((start.X, start.Y, start.Z));
		visited.Add((start.X, start.Y, start.Z));

		bool foundEnd = false;
		while (queue.Count > 0 && !foundEnd)
		{
			var current = queue.Dequeue();
			
			if (current.x == end.X && current.y == end.Y && current.z == end.Z)
			{
				foundEnd = true;
				break;
			}

			var point = new MazePoint(current.x, current.y, current.z);
			mazeJumper.JumpToPoint(point);
			var directions = mazeJumper.GetFlagFromPoint();

			TryVisitNeighbor(directions, Direction.Left, current.x - 1, current.y, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Right, current.x + 1, current.y, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Back, current.x, current.y - 1, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Forward, current.x, current.y + 1, current.z, visited, queue);
			TryVisitNeighbor(directions, Direction.Down, current.x, current.y, current.z - 1, visited, queue);
			TryVisitNeighbor(directions, Direction.Up, current.x, current.y, current.z + 1, visited, queue);
		}

		// Assert
		Assert.That(foundEnd, Is.True, 
			$"End point ({end.X},{end.Y},{end.Z}) should be reachable from start ({start.X},{start.Y},{start.Z})");
	}

	[Test]
	public void GenerateMaze_ShortestPathMatchesFloodFillDistance()
	{
		// Arrange
		var settings = CreateDefaultSettings(10, 10, 1);
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var mazeJumper = result.MazeJumper;

		// Act - BFS from start to find shortest path to end
		var distances = new Dictionary<(int x, int y, int z), int>();
		var queue = new Queue<(int x, int y, int z)>();
		
		var start = mazeJumper.StartPoint;
		var end = mazeJumper.EndPoint;
		
		queue.Enqueue((start.X, start.Y, start.Z));
		distances[(start.X, start.Y, start.Z)] = 0;

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			var currentDist = distances[current];

			var point = new MazePoint(current.x, current.y, current.z);
			mazeJumper.JumpToPoint(point);
			var directions = mazeJumper.GetFlagFromPoint();

			TryVisitWithDistance(directions, Direction.Left, current.x - 1, current.y, current.z, currentDist + 1, distances, queue);
			TryVisitWithDistance(directions, Direction.Right, current.x + 1, current.y, current.z, currentDist + 1, distances, queue);
			TryVisitWithDistance(directions, Direction.Back, current.x, current.y - 1, current.z, currentDist + 1, distances, queue);
			TryVisitWithDistance(directions, Direction.Forward, current.x, current.y + 1, current.z, currentDist + 1, distances, queue);
			TryVisitWithDistance(directions, Direction.Down, current.x, current.y, current.z - 1, currentDist + 1, distances, queue);
			TryVisitWithDistance(directions, Direction.Up, current.x, current.y, current.z + 1, currentDist + 1, distances, queue);
		}

		var bfsDistance = distances[(end.X, end.Y, end.Z)];
		var reportedPath = result.HeuristicsResults.ShortestPathResult.ShortestPath;

		// Assert
		Assert.That(bfsDistance, Is.EqualTo(reportedPath), 
			$"BFS distance ({bfsDistance}) should match reported shortest path ({reportedPath})");
	}

	private void TryVisitNeighbor(Direction directions, Direction requiredDir, 
		int x, int y, int z, 
		HashSet<(int x, int y, int z)> visited, 
		Queue<(int x, int y, int z)> queue)
	{
		if ((directions & requiredDir) != 0 && !visited.Contains((x, y, z)))
		{
			visited.Add((x, y, z));
			queue.Enqueue((x, y, z));
		}
	}

	private void TryVisitWithDistance(Direction directions, Direction requiredDir,
		int x, int y, int z, int distance,
		Dictionary<(int x, int y, int z), int> distances,
		Queue<(int x, int y, int z)> queue)
	{
		if ((directions & requiredDir) != 0 && !distances.ContainsKey((x, y, z)))
		{
			distances[(x, y, z)] = distance;
			queue.Enqueue((x, y, z));
		}
	}

	[Test]
	public void DijkstraAnimator_ProducesValidDistances()
	{
		// Arrange
		var settings = CreateDefaultSettings(5, 5, 1);
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var graph = result.HeuristicsResults.ShortestPathResult.Graph;
		var mazeJumper = result.MazeJumper;
		
		// Act - run the animator
		var animator = _services.DijkstraAnimator;
		var steps = animator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);
		
		// Assert - should have steps
		Assert.That(steps.Count, Is.GreaterThan(0), "Should have at least one step");
		
		// First step should be Initialize with start distance = 0
		var initStep = steps[0];
		Assert.That(initStep.Type, Is.EqualTo(ProceduralMaze.Maze.Solver.StepType.Initialize));
		
		// Verify start point has distance 0 in the first step
		var startPoint = mazeJumper.StartPoint;
		Assert.That(initStep.Distances.ContainsKey(startPoint), Is.True, 
			$"Distances should contain start point ({startPoint.X},{startPoint.Y},{startPoint.Z})");
		Assert.That(initStep.Distances[startPoint], Is.EqualTo(0), 
			"Start point should have distance 0");
		
		// Last step should be Complete
		var lastStep = steps[steps.Count - 1];
		Assert.That(lastStep.Type, Is.EqualTo(ProceduralMaze.Maze.Solver.StepType.Complete));
		
		// End point should have a finite distance in the last step
		var endPoint = mazeJumper.EndPoint;
		Assert.That(lastStep.Distances.ContainsKey(endPoint), Is.True,
			$"Distances should contain end point ({endPoint.X},{endPoint.Y},{endPoint.Z})");
		Assert.That(lastStep.Distances[endPoint], Is.LessThan(int.MaxValue),
			"End point should have finite distance");
	}
	
	[Test]
	public void DijkstraAnimator_AllStepsHaveNonEmptyDistances()
	{
		// Arrange
		var settings = CreateDefaultSettings(5, 5, 1);
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var graph = result.HeuristicsResults.ShortestPathResult.Graph;
		var mazeJumper = result.MazeJumper;
		
		// Act
		var animator = _services.DijkstraAnimator;
		var steps = animator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);
		
		// Assert - every step should have distances for all graph nodes
		for (int i = 0; i < steps.Count; i++)
		{
			var step = steps[i];
			Assert.That(step.Distances, Is.Not.Null, $"Step {i} Distances should not be null");
			Assert.That(step.Distances.Count, Is.EqualTo(graph.Nodes.Count), 
				$"Step {i} should have distances for all {graph.Nodes.Count} nodes, but has {step.Distances.Count}");
		}
	}
	
	[Test]
	public void DijkstraAnimator_GraphNodeKeysMatchDistanceKeys()
	{
		// Arrange
		var settings = CreateDefaultSettings(5, 5, 1);
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var graph = result.HeuristicsResults.ShortestPathResult.Graph;
		var mazeJumper = result.MazeJumper;
		
		// Act
		var animator = _services.DijkstraAnimator;
		var steps = animator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);
		
		// Get the first step's distances
		var initStep = steps[0];
		
		// For each graph node, try to look it up in the step's distances
		// This simulates what the renderer does
		int foundCount = 0;
		int notFoundCount = 0;
		foreach (var kvp in graph.Nodes)
		{
			var point = kvp.Key;
			if (initStep.Distances.ContainsKey(point))
			{
				foundCount++;
			}
			else
			{
				notFoundCount++;
				TestContext.WriteLine($"Graph node ({point.X},{point.Y},{point.Z}) not found in Distances");
			}
		}
		
		Assert.That(notFoundCount, Is.EqualTo(0), 
			$"All {graph.Nodes.Count} graph nodes should be found in Distances, but {notFoundCount} were missing");
	}

	[Test]
	public void DijkstraAnimator_DistancesMatchGraphNodes()
	{
		// Arrange
		var settings = CreateDefaultSettings(5, 5, 1);
		var result = _services.MazeGenerationFactory.GenerateMaze(settings);
		var graph = result.HeuristicsResults.ShortestPathResult.Graph;
		var mazeJumper = result.MazeJumper;
		
		// Act - run the animator
		var animator = _services.DijkstraAnimator;
		var steps = animator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);
		
		// Get the last step
		var lastStep = steps[steps.Count - 1];
		
		// Assert - every graph node key should be in the Distances dictionary
		foreach (var nodeKey in graph.Nodes.Keys)
		{
			Assert.That(lastStep.Distances.ContainsKey(nodeKey), Is.True,
				$"Distances should contain graph node at ({nodeKey.X},{nodeKey.Y},{nodeKey.Z})");
		}
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
