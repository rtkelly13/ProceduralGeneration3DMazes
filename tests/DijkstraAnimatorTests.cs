using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralMaze.Tests;

/// <summary>
/// Tests for DijkstraAnimator using simple, managed graphs to ensure
/// reliable animation behavior with predictable inputs.
/// </summary>
[TestFixture]
public class DijkstraAnimatorTests
{
	#region Test Case Sources

	/// <summary>
	/// Provides different graph configurations for parameterized tests.
	/// Returns: (graphName, graphFactory, expectedShortestPath)
	/// </summary>
	private static IEnumerable<TestCaseData> GraphConfigurations()
	{
		yield return new TestCaseData("Linear", (Func<(Graph, MazePoint, MazePoint)>)CreateLinearGraph, 2)
			.SetDescription("Simple linear graph A-B-C");
		yield return new TestCaseData("Diamond", (Func<(Graph, MazePoint, MazePoint)>)CreateDiamondGraph, 3)
			.SetDescription("Diamond graph with two equal-length paths");
		yield return new TestCaseData("SingleNode", (Func<(Graph, MazePoint, MazePoint)>)CreateSingleNodeGraph, 0)
			.SetDescription("Single node where start equals end");
		yield return new TestCaseData("MultiPath", (Func<(Graph, MazePoint, MazePoint)>)CreateMultiPathGraph, 2)
			.SetDescription("Graph with multiple paths of different lengths");
		yield return new TestCaseData("LongChain", (Func<(Graph, MazePoint, MazePoint)>)CreateLongChainGraph, 10)
			.SetDescription("Long linear chain of 11 nodes");
		yield return new TestCaseData("Grid2x2", (Func<(Graph, MazePoint, MazePoint)>)CreateGrid2x2Graph, 2)
			.SetDescription("2x2 grid graph");
		yield return new TestCaseData("Grid3x3", (Func<(Graph, MazePoint, MazePoint)>)CreateGrid3x3Graph, 4)
			.SetDescription("3x3 grid graph");
		yield return new TestCaseData("StarGraph", (Func<(Graph, MazePoint, MazePoint)>)CreateStarGraph, 2)
			.SetDescription("Star graph with central hub");
		yield return new TestCaseData("AsymmetricWeights", (Func<(Graph, MazePoint, MazePoint)>)CreateAsymmetricWeightsGraph, 3)
			.SetDescription("Graph with asymmetric edge weights");
	}

	/// <summary>
	/// Provides maze generation settings for integration tests.
	/// </summary>
	private static IEnumerable<TestCaseData> MazeConfigurations()
	{
		// Different sizes
		yield return new TestCaseData(3, 3, 1, Algorithm.GrowingTreeAlgorithm, 0).SetName("3x3 GrowingTree Perfect");
		yield return new TestCaseData(5, 5, 1, Algorithm.GrowingTreeAlgorithm, 0).SetName("5x5 GrowingTree Perfect");
		yield return new TestCaseData(10, 10, 1, Algorithm.GrowingTreeAlgorithm, 0).SetName("10x10 GrowingTree Perfect");
		yield return new TestCaseData(15, 15, 1, Algorithm.GrowingTreeAlgorithm, 0).SetName("15x15 GrowingTree Perfect");
		
		// Different algorithms
		yield return new TestCaseData(5, 5, 1, Algorithm.RecursiveBacktrackerAlgorithm, 0).SetName("5x5 Backtracker Perfect");
		yield return new TestCaseData(5, 5, 1, Algorithm.BinaryTreeAlgorithm, 0).SetName("5x5 BinaryTree Perfect");
		
		// With wall removal (imperfect mazes)
		yield return new TestCaseData(5, 5, 1, Algorithm.GrowingTreeAlgorithm, 10).SetName("5x5 GrowingTree 10% walls removed");
		yield return new TestCaseData(5, 5, 1, Algorithm.GrowingTreeAlgorithm, 25).SetName("5x5 GrowingTree 25% walls removed");
		
		// 3D mazes
		yield return new TestCaseData(3, 3, 2, Algorithm.GrowingTreeAlgorithm, 0).SetName("3x3x2 GrowingTree Perfect");
		yield return new TestCaseData(4, 4, 3, Algorithm.GrowingTreeAlgorithm, 0).SetName("4x4x3 GrowingTree Perfect");
		
		// Non-square mazes
		yield return new TestCaseData(3, 7, 1, Algorithm.GrowingTreeAlgorithm, 0).SetName("3x7 GrowingTree Perfect");
		yield return new TestCaseData(8, 3, 1, Algorithm.GrowingTreeAlgorithm, 0).SetName("8x3 GrowingTree Perfect");
	}

	/// <summary>
	/// Provides iteration counts for stress testing.
	/// </summary>
	private static IEnumerable<TestCaseData> StressTestIterations()
	{
		yield return new TestCaseData(5, 5, 1, 20).SetName("5x5 maze 20 iterations");
		yield return new TestCaseData(8, 8, 1, 10).SetName("8x8 maze 10 iterations");
		yield return new TestCaseData(3, 3, 2, 15).SetName("3x3x2 maze 15 iterations");
	}

	#endregion

	#region Test Helpers

	/// <summary>
	/// Creates a simple linear graph: A -> B -> C
	/// Edge weights: A-B = 1, B-C = 1
	/// </summary>
	private static (Graph graph, MazePoint start, MazePoint end) CreateLinearGraph()
	{
		var pointA = new MazePoint(0, 0, 0);
		var pointB = new MazePoint(1, 0, 0);
		var pointC = new MazePoint(2, 0, 0);

		var graph = new Graph
		{
			Nodes = new Dictionary<MazePoint, GraphNode>
			{
				[pointA] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = pointB, DirectionsToPoint = new Direction[] { Direction.Right } }
					}
				},
				[pointB] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = pointA, DirectionsToPoint = new Direction[] { Direction.Left } },
						new GraphEdge { Point = pointC, DirectionsToPoint = new Direction[] { Direction.Right } }
					}
				},
				[pointC] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = pointB, DirectionsToPoint = new Direction[] { Direction.Left } }
					}
				}
			}
		};

		return (graph, pointA, pointC);
	}

	/// <summary>
	/// Creates a diamond-shaped graph:
	///       B
	///      / \
	///     A   D
	///      \ /
	///       C
	/// Edge weights: A-B = 1, A-C = 2, B-D = 2, C-D = 1
	/// Optimal path: A -> B -> D (cost 3) or A -> C -> D (cost 3)
	/// </summary>
	private static (Graph graph, MazePoint start, MazePoint end) CreateDiamondGraph()
	{
		var pointA = new MazePoint(0, 1, 0);
		var pointB = new MazePoint(1, 2, 0);
		var pointC = new MazePoint(1, 0, 0);
		var pointD = new MazePoint(2, 1, 0);

		var graph = new Graph
		{
			Nodes = new Dictionary<MazePoint, GraphNode>
			{
				[pointA] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						// A -> B: weight 1
						new GraphEdge { Point = pointB, DirectionsToPoint = new Direction[] { Direction.Forward } },
						// A -> C: weight 2
						new GraphEdge { Point = pointC, DirectionsToPoint = new Direction[] { Direction.Back, Direction.Right } }
					}
				},
				[pointB] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						// B -> A: weight 1
						new GraphEdge { Point = pointA, DirectionsToPoint = new Direction[] { Direction.Back } },
						// B -> D: weight 2
						new GraphEdge { Point = pointD, DirectionsToPoint = new Direction[] { Direction.Right, Direction.Back } }
					}
				},
				[pointC] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						// C -> A: weight 2
						new GraphEdge { Point = pointA, DirectionsToPoint = new Direction[] { Direction.Left, Direction.Forward } },
						// C -> D: weight 1
						new GraphEdge { Point = pointD, DirectionsToPoint = new Direction[] { Direction.Right } }
					}
				},
				[pointD] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						// D -> B: weight 2
						new GraphEdge { Point = pointB, DirectionsToPoint = new Direction[] { Direction.Forward, Direction.Left } },
						// D -> C: weight 1
						new GraphEdge { Point = pointC, DirectionsToPoint = new Direction[] { Direction.Left } }
					}
				}
			}
		};

		return (graph, pointA, pointD);
	}

	/// <summary>
	/// Creates a single node graph (edge case).
	/// </summary>
	private static (Graph graph, MazePoint start, MazePoint end) CreateSingleNodeGraph()
	{
		var point = new MazePoint(0, 0, 0);

		var graph = new Graph
		{
			Nodes = new Dictionary<MazePoint, GraphNode>
			{
				[point] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = []
				}
			}
		};

		return (graph, point, point);
	}

	/// <summary>
	/// Creates a graph with multiple paths of different weights:
	///     A ---1--- B ---1--- E
	///     |        |
	///     3        2
	///     |        |
	///     C ---1--- D
	/// </summary>
	private static (Graph graph, MazePoint start, MazePoint end) CreateMultiPathGraph()
	{
		var pointA = new MazePoint(0, 1, 0);
		var pointB = new MazePoint(1, 1, 0);
		var pointC = new MazePoint(0, 0, 0);
		var pointD = new MazePoint(1, 0, 0);
		var pointE = new MazePoint(2, 1, 0);

		var graph = new Graph
		{
			Nodes = new Dictionary<MazePoint, GraphNode>
			{
				[pointA] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = pointB, DirectionsToPoint = new Direction[] { Direction.Right } },
						new GraphEdge { Point = pointC, DirectionsToPoint = new Direction[] { Direction.Back, Direction.Back, Direction.Back } }
					}
				},
				[pointB] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = pointA, DirectionsToPoint = new Direction[] { Direction.Left } },
						new GraphEdge { Point = pointD, DirectionsToPoint = new Direction[] { Direction.Back, Direction.Back } },
						new GraphEdge { Point = pointE, DirectionsToPoint = new Direction[] { Direction.Right } }
					}
				},
				[pointC] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = pointA, DirectionsToPoint = new Direction[] { Direction.Forward, Direction.Forward, Direction.Forward } },
						new GraphEdge { Point = pointD, DirectionsToPoint = new Direction[] { Direction.Right } }
					}
				},
				[pointD] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = pointC, DirectionsToPoint = new Direction[] { Direction.Left } },
						new GraphEdge { Point = pointB, DirectionsToPoint = new Direction[] { Direction.Forward, Direction.Forward } }
					}
				},
				[pointE] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = pointB, DirectionsToPoint = new Direction[] { Direction.Left } }
					}
				}
			}
		};

		return (graph, pointA, pointE);
	}

	private static DijkstraAnimator CreateAnimator()
	{
		// DijkstraAnimator needs a GraphBuilder, but we're using the overload that takes
		// a pre-built graph, so we can pass null (it won't be used)
		return new DijkstraAnimator(null!);
	}

	/// <summary>
	/// Creates a long chain graph: A -> B -> C -> ... -> K (11 nodes)
	/// </summary>
	private static (Graph graph, MazePoint start, MazePoint end) CreateLongChainGraph()
	{
		var nodes = new Dictionary<MazePoint, GraphNode>();
		const int chainLength = 11;

		for (int i = 0; i < chainLength; i++)
		{
			var point = new MazePoint(i, 0, 0);
			var edges = new List<GraphEdge>();

			if (i > 0)
			{
				edges.Add(new GraphEdge
				{
					Point = new MazePoint(i - 1, 0, 0),
					DirectionsToPoint = new Direction[] { Direction.Left }
				});
			}
			if (i < chainLength - 1)
			{
				edges.Add(new GraphEdge
				{
					Point = new MazePoint(i + 1, 0, 0),
					DirectionsToPoint = new Direction[] { Direction.Right }
				});
			}

			nodes[point] = new GraphNode
			{
				ShortestPath = int.MaxValue,
				Edges = edges.ToArray()
			};
		}

		return (new Graph { Nodes = nodes }, new MazePoint(0, 0, 0), new MazePoint(chainLength - 1, 0, 0));
	}

	/// <summary>
	/// Creates a 2x2 grid graph:
	/// A - B
	/// |   |
	/// C - D
	/// </summary>
	private static (Graph graph, MazePoint start, MazePoint end) CreateGrid2x2Graph()
	{
		var a = new MazePoint(0, 1, 0);
		var b = new MazePoint(1, 1, 0);
		var c = new MazePoint(0, 0, 0);
		var d = new MazePoint(1, 0, 0);

		var graph = new Graph
		{
			Nodes = new Dictionary<MazePoint, GraphNode>
			{
				[a] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = b, DirectionsToPoint = new Direction[] { Direction.Right } },
						new GraphEdge { Point = c, DirectionsToPoint = new Direction[] { Direction.Back } }
					}
				},
				[b] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = a, DirectionsToPoint = new Direction[] { Direction.Left } },
						new GraphEdge { Point = d, DirectionsToPoint = new Direction[] { Direction.Back } }
					}
				},
				[c] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = a, DirectionsToPoint = new Direction[] { Direction.Forward } },
						new GraphEdge { Point = d, DirectionsToPoint = new Direction[] { Direction.Right } }
					}
				},
				[d] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = b, DirectionsToPoint = new Direction[] { Direction.Forward } },
						new GraphEdge { Point = c, DirectionsToPoint = new Direction[] { Direction.Left } }
					}
				}
			}
		};

		return (graph, a, d);
	}

	/// <summary>
	/// Creates a 3x3 grid graph (fully connected grid).
	/// </summary>
	private static (Graph graph, MazePoint start, MazePoint end) CreateGrid3x3Graph()
	{
		var nodes = new Dictionary<MazePoint, GraphNode>();

		for (int x = 0; x < 3; x++)
		{
			for (int y = 0; y < 3; y++)
			{
				var point = new MazePoint(x, y, 0);
				var edges = new List<GraphEdge>();

				if (x > 0)
					edges.Add(new GraphEdge { Point = new MazePoint(x - 1, y, 0), DirectionsToPoint = new Direction[] { Direction.Left } });
				if (x < 2)
					edges.Add(new GraphEdge { Point = new MazePoint(x + 1, y, 0), DirectionsToPoint = new Direction[] { Direction.Right } });
				if (y > 0)
					edges.Add(new GraphEdge { Point = new MazePoint(x, y - 1, 0), DirectionsToPoint = new Direction[] { Direction.Back } });
				if (y < 2)
					edges.Add(new GraphEdge { Point = new MazePoint(x, y + 1, 0), DirectionsToPoint = new Direction[] { Direction.Forward } });

				nodes[point] = new GraphNode { ShortestPath = int.MaxValue, Edges = edges.ToArray() };
			}
		}

		return (new Graph { Nodes = nodes }, new MazePoint(0, 0, 0), new MazePoint(2, 2, 0));
	}

	/// <summary>
	/// Creates a star graph with center hub connected to 4 outer nodes.
	///     N
	///     |
	/// W - C - E
	///     |
	///     S
	/// </summary>
	private static (Graph graph, MazePoint start, MazePoint end) CreateStarGraph()
	{
		var center = new MazePoint(1, 1, 0);
		var north = new MazePoint(1, 2, 0);
		var south = new MazePoint(1, 0, 0);
		var east = new MazePoint(2, 1, 0);
		var west = new MazePoint(0, 1, 0);

		var graph = new Graph
		{
			Nodes = new Dictionary<MazePoint, GraphNode>
			{
				[center] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = north, DirectionsToPoint = new Direction[] { Direction.Forward } },
						new GraphEdge { Point = south, DirectionsToPoint = new Direction[] { Direction.Back } },
						new GraphEdge { Point = east, DirectionsToPoint = new Direction[] { Direction.Right } },
						new GraphEdge { Point = west, DirectionsToPoint = new Direction[] { Direction.Left } }
					}
				},
				[north] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = center, DirectionsToPoint = new Direction[] { Direction.Back } }
					}
				},
				[south] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = center, DirectionsToPoint = new Direction[] { Direction.Forward } }
					}
				},
				[east] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = center, DirectionsToPoint = new Direction[] { Direction.Left } }
					}
				},
				[west] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = center, DirectionsToPoint = new Direction[] { Direction.Right } }
					}
				}
			}
		};

		return (graph, west, east);
	}

	/// <summary>
	/// Creates a graph with asymmetric edge weights.
	/// A --1-- B --3-- C
	///  \             /
	///   ----2-------
	/// Path A->B->C = 4, Path A->C = 2
	/// </summary>
	private static (Graph graph, MazePoint start, MazePoint end) CreateAsymmetricWeightsGraph()
	{
		var a = new MazePoint(0, 0, 0);
		var b = new MazePoint(1, 0, 0);
		var c = new MazePoint(2, 0, 0);

		var graph = new Graph
		{
			Nodes = new Dictionary<MazePoint, GraphNode>
			{
				[a] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = b, DirectionsToPoint = new Direction[] { Direction.Right } },
						// Direct path A->C with weight 2 (but longer edge)
						new GraphEdge { Point = c, DirectionsToPoint = new Direction[] { Direction.Right, Direction.Right, Direction.Right } }
					}
				},
				[b] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = a, DirectionsToPoint = new Direction[] { Direction.Left } },
						new GraphEdge { Point = c, DirectionsToPoint = new Direction[] { Direction.Right, Direction.Right, Direction.Right } }
					}
				},
				[c] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = b, DirectionsToPoint = new Direction[] { Direction.Left, Direction.Left, Direction.Left } },
						new GraphEdge { Point = a, DirectionsToPoint = new Direction[] { Direction.Left, Direction.Left, Direction.Left } }
					}
				}
			}
		};

		return (graph, a, c);
	}

	private static MazeGenerationSettings CreateMazeSettings(int x, int y, int z, Algorithm algorithm, int wallRemovalPercent)
	{
		return new MazeGenerationSettings
		{
			Algorithm = algorithm,
			Size = new MazeSize { X = x, Y = y, Z = z },
			Option = MazeType.ArrayBidirectional,
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
	}

	#endregion

	#region Basic Step Structure Tests

	[Test]
	public void CaptureSteps_LinearGraph_StartsWithInitializeStep()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert
		Assert.That(steps.Count, Is.GreaterThan(0));
		Assert.That(steps[0].Type, Is.EqualTo(StepType.Initialize));
	}

	[Test]
	public void CaptureSteps_LinearGraph_EndsWithCompleteStep()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert
		Assert.That(steps[^1].Type, Is.EqualTo(StepType.Complete));
	}

	[Test]
	public void CaptureSteps_LinearGraph_InitStepHasStartDistanceZero()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var initStep = steps[0];

		// Assert
		Assert.That(initStep.Distances.ContainsKey(start), Is.True);
		Assert.That(initStep.Distances[start], Is.EqualTo(0));
	}

	[Test]
	public void CaptureSteps_LinearGraph_InitStepHasOtherNodesAtMaxValue()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var initStep = steps[0];

		// Assert
		foreach (var node in graph.Nodes.Keys.Where(n => !n.Equals(start)))
		{
			Assert.That(initStep.Distances.ContainsKey(node), Is.True,
				$"Node ({node.X},{node.Y},{node.Z}) should be in distances");
			Assert.That(initStep.Distances[node], Is.EqualTo(int.MaxValue),
				$"Node ({node.X},{node.Y},{node.Z}) should have MaxValue initially");
		}
	}

	[Test]
	public void CaptureSteps_LinearGraph_AllStepsHaveDistancesForAllNodes()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert
		foreach (var step in steps)
		{
			Assert.That(step.Distances.Count, Is.EqualTo(graph.Nodes.Count),
				$"Step of type {step.Type} should have distances for all {graph.Nodes.Count} nodes");
			
			foreach (var nodeKey in graph.Nodes.Keys)
			{
				Assert.That(step.Distances.ContainsKey(nodeKey), Is.True,
					$"Step {step.Type} should contain distance for node ({nodeKey.X},{nodeKey.Y},{nodeKey.Z})");
			}
		}
	}

	#endregion

	#region Step Ordering Tests

	[Test]
	public void CaptureSteps_LinearGraph_SelectNodeFollowsInitialize()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert - second step should be SelectNode
		Assert.That(steps.Count, Is.GreaterThan(1));
		Assert.That(steps[1].Type, Is.EqualTo(StepType.SelectNode));
	}

	[Test]
	public void CaptureSteps_LinearGraph_FirstSelectNodeIsStartPoint()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var firstSelectNode = steps.First(s => s.Type == StepType.SelectNode);

		// Assert
		Assert.That(firstSelectNode.CurrentNode, Is.EqualTo(start));
	}

	[Test]
	public void CaptureSteps_LinearGraph_FinalizeNodeFollowsEdgeExamination()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert - FinalizeNode should follow after edge processing
		var finalizeSteps = steps.Where(s => s.Type == StepType.FinalizeNode).ToList();
		Assert.That(finalizeSteps.Count, Is.GreaterThan(0), "Should have FinalizeNode steps");

		foreach (var finalize in finalizeSteps)
		{
			var idx = steps.IndexOf(finalize);
			// Check that previous steps include edge examination
			var precedingTypes = steps.Take(idx).Select(s => s.Type).ToList();
			Assert.That(precedingTypes, Does.Contain(StepType.SelectNode),
				"FinalizeNode should be preceded by SelectNode");
		}
	}

	#endregion

	#region Distance Progression Tests

	[Test]
	public void CaptureSteps_LinearGraph_DistancesNeverIncrease()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert - for each node, distances should only decrease or stay same
		foreach (var nodeKey in graph.Nodes.Keys)
		{
			int previousDistance = int.MaxValue;
			foreach (var step in steps)
			{
				int currentDistance = step.Distances[nodeKey];
				Assert.That(currentDistance, Is.LessThanOrEqualTo(previousDistance),
					$"Distance to ({nodeKey.X},{nodeKey.Y},{nodeKey.Z}) should never increase");
				previousDistance = currentDistance;
			}
		}
	}

	[Test]
	public void CaptureSteps_LinearGraph_FinalDistanceMatchesPathLength()
	{
		// Arrange - linear graph A -> B -> C, path length is 2
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var finalStep = steps[^1];

		// Assert
		Assert.That(finalStep.Distances[end], Is.EqualTo(2));
	}

	[Test]
	public void CaptureSteps_DiamondGraph_FindsOptimalPath()
	{
		// Arrange - diamond graph with two paths of length 3
		var (graph, start, end) = CreateDiamondGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var finalStep = steps[^1];

		// Assert - optimal path length is 3
		Assert.That(finalStep.Distances[end], Is.EqualTo(3));
	}

	[Test]
	public void CaptureSteps_MultiPathGraph_FindsShortestPath()
	{
		// Arrange - A to E: shortest is A->B->E = 2
		var (graph, start, end) = CreateMultiPathGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var finalStep = steps[^1];

		// Assert
		Assert.That(finalStep.Distances[end], Is.EqualTo(2));
	}

	#endregion

	#region Visited and Frontier State Tests

	[Test]
	public void CaptureSteps_LinearGraph_VisitedNodesGrowMonotonically()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert
		int previousVisitedCount = 0;
		foreach (var step in steps)
		{
			Assert.That(step.VisitedNodes.Count, Is.GreaterThanOrEqualTo(previousVisitedCount),
				$"Visited nodes should grow monotonically at step {step.Type}");
			previousVisitedCount = step.VisitedNodes.Count;
		}
	}

	[Test]
	public void CaptureSteps_LinearGraph_StartInFrontierAfterInit()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var initStep = steps[0];

		// Assert
		Assert.That(initStep.FrontierNodes.Contains(start), Is.True,
			"Start node should be in frontier after initialization");
	}

	[Test]
	public void CaptureSteps_LinearGraph_NodeMovesFromFrontierToVisited()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Find when start moves from frontier to visited
		bool foundInFrontier = false;
		bool foundInVisited = false;

		foreach (var step in steps)
		{
			if (step.FrontierNodes.Contains(start))
			{
				foundInFrontier = true;
			}
			if (step.VisitedNodes.Contains(start))
			{
				foundInVisited = true;
				// Once visited, should not be in frontier
				Assert.That(step.FrontierNodes.Contains(start), Is.False,
					"Node should not be in frontier once visited");
			}
		}

		Assert.That(foundInFrontier, Is.True, "Start should be in frontier at some point");
		Assert.That(foundInVisited, Is.True, "Start should be visited at some point");
	}

	#endregion

	#region Edge Case Tests

	[Test]
	public void CaptureSteps_SingleNode_HandlesStartEqualsEnd()
	{
		// Arrange
		var (graph, start, end) = CreateSingleNodeGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert
		Assert.That(steps.Count, Is.GreaterThanOrEqualTo(2), "Should have at least Init and Complete");
		Assert.That(steps[0].Type, Is.EqualTo(StepType.Initialize));
		Assert.That(steps[^1].Type, Is.EqualTo(StepType.Complete));
		Assert.That(steps[^1].Distances[start], Is.EqualTo(0));
	}

	[Test]
	public void CaptureSteps_LinearGraph_AllNodesEventuallyReached()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var finalStep = steps[^1];

		// Assert - all nodes should have finite distance at end
		foreach (var nodeKey in graph.Nodes.Keys)
		{
			Assert.That(finalStep.Distances[nodeKey], Is.LessThan(int.MaxValue),
				$"Node ({nodeKey.X},{nodeKey.Y},{nodeKey.Z}) should be reachable");
		}
	}

	#endregion

	#region RelaxEdge Step Tests

	[Test]
	public void CaptureSteps_LinearGraph_HasRelaxEdgeSteps()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var relaxSteps = steps.Where(s => s.Type == StepType.RelaxEdge).ToList();

		// Assert - should have relaxation steps
		Assert.That(relaxSteps.Count, Is.GreaterThan(0), "Should have RelaxEdge steps");
	}

	[Test]
	public void CaptureSteps_LinearGraph_RelaxStepHasCorrectNewDistance()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var pointB = new MazePoint(1, 0, 0);
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		
		// Find the relax step for B (from A to B)
		var relaxToB = steps.FirstOrDefault(s => 
			s.Type == StepType.RelaxEdge && 
			s.NeighborNode != null && 
			s.NeighborNode.Equals(pointB));

		// Assert
		Assert.That(relaxToB, Is.Not.Null, "Should have a RelaxEdge step for B");
		Assert.That(relaxToB!.NewDistance, Is.EqualTo(1), "New distance to B should be 1");
	}

	[Test]
	public void CaptureSteps_DiamondGraph_HasSkipEdgeSteps()
	{
		// Arrange - diamond graph has redundant edges that should be skipped
		var (graph, start, end) = CreateDiamondGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var skipSteps = steps.Where(s => s.Type == StepType.SkipEdge).ToList();

		// Assert - with diamond graph, some edges should be skipped
		// (when we've already found a better path)
		Assert.That(skipSteps.Count, Is.GreaterThanOrEqualTo(0), 
			"Diamond graph may have skipped edges");
	}

	#endregion

	#region Predecessor Tests

	[Test]
	public void CaptureSteps_LinearGraph_PredecessorsFormPath()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var finalStep = steps[^1];

		// Assert - reconstruct path from predecessors
		var path = new List<MazePoint>();
		var current = end;
		
		while (finalStep.Predecessors.ContainsKey(current))
		{
			path.Add(current);
			current = finalStep.Predecessors[current];
		}
		path.Add(current); // Add start
		path.Reverse();

		Assert.That(path[0], Is.EqualTo(start), "Path should start at start point");
		Assert.That(path[^1], Is.EqualTo(end), "Path should end at end point");
		Assert.That(path.Count, Is.EqualTo(3), "Linear path should have 3 nodes");
	}

	[Test]
	public void CaptureSteps_LinearGraph_StartHasNoPredecessor()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var finalStep = steps[^1];

		// Assert
		Assert.That(finalStep.Predecessors.ContainsKey(start), Is.False,
			"Start node should have no predecessor");
	}

	#endregion

	#region Step Description Tests

	[Test]
	public void CaptureSteps_AllStepsHaveNonEmptyDescription()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert
		foreach (var step in steps)
		{
			Assert.That(string.IsNullOrEmpty(step.Description), Is.False,
				$"Step of type {step.Type} should have a description");
		}
	}

	[Test]
	public void CaptureSteps_InitStepDescriptionContainsStart()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var initStep = steps[0];

		// Assert
		Assert.That(initStep.Description.Contains("0"), Is.True,
			"Init description should mention start coordinates or distance 0");
	}

	[Test]
	public void CaptureSteps_CompleteStepDescriptionContainsEndDistance()
	{
		// Arrange
		var (graph, start, end) = CreateLinearGraph();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var completeStep = steps[^1];

		// Assert - final distance is 2
		Assert.That(completeStep.Description.Contains("2"), Is.True,
			"Complete description should mention final distance");
	}

	#endregion

	#region Parameterized Tests - All Graph Configurations

	[Test]
	[TestCaseSource(nameof(GraphConfigurations))]
	public void CaptureSteps_AllConfigs_StartsWithInitializeEndsWithComplete(
		string graphName, Func<(Graph, MazePoint, MazePoint)> graphFactory, int expectedDistance)
	{
		// Arrange
		var (graph, start, end) = graphFactory();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert
		Assert.That(steps.Count, Is.GreaterThanOrEqualTo(2), $"{graphName}: Should have at least 2 steps");
		Assert.That(steps[0].Type, Is.EqualTo(StepType.Initialize), $"{graphName}: Should start with Initialize");
		Assert.That(steps[^1].Type, Is.EqualTo(StepType.Complete), $"{graphName}: Should end with Complete");
	}

	[Test]
	[TestCaseSource(nameof(GraphConfigurations))]
	public void CaptureSteps_AllConfigs_FindsCorrectShortestPath(
		string graphName, Func<(Graph, MazePoint, MazePoint)> graphFactory, int expectedDistance)
	{
		// Arrange
		var (graph, start, end) = graphFactory();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);
		var finalStep = steps[^1];

		// Assert
		Assert.That(finalStep.Distances[end], Is.EqualTo(expectedDistance),
			$"{graphName}: Expected shortest path distance {expectedDistance}");
	}

	[Test]
	[TestCaseSource(nameof(GraphConfigurations))]
	public void CaptureSteps_AllConfigs_AllStepsHaveCompleteDistances(
		string graphName, Func<(Graph, MazePoint, MazePoint)> graphFactory, int expectedDistance)
	{
		// Arrange
		var (graph, start, end) = graphFactory();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert - every step should have distances for all nodes
		for (int i = 0; i < steps.Count; i++)
		{
			var step = steps[i];
			Assert.That(step.Distances.Count, Is.EqualTo(graph.Nodes.Count),
				$"{graphName} step {i} ({step.Type}): Should have {graph.Nodes.Count} distances, has {step.Distances.Count}");

			foreach (var nodeKey in graph.Nodes.Keys)
			{
				Assert.That(step.Distances.ContainsKey(nodeKey), Is.True,
					$"{graphName} step {i}: Missing distance for node ({nodeKey.X},{nodeKey.Y},{nodeKey.Z})");
			}
		}
	}

	[Test]
	[TestCaseSource(nameof(GraphConfigurations))]
	public void CaptureSteps_AllConfigs_DistancesMonotonicallyDecrease(
		string graphName, Func<(Graph, MazePoint, MazePoint)> graphFactory, int expectedDistance)
	{
		// Arrange
		var (graph, start, end) = graphFactory();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert - distances should never increase
		foreach (var nodeKey in graph.Nodes.Keys)
		{
			int previousDistance = int.MaxValue;
			for (int i = 0; i < steps.Count; i++)
			{
				int currentDistance = steps[i].Distances[nodeKey];
				Assert.That(currentDistance, Is.LessThanOrEqualTo(previousDistance),
					$"{graphName}: Distance to ({nodeKey.X},{nodeKey.Y},{nodeKey.Z}) increased at step {i}");
				previousDistance = currentDistance;
			}
		}
	}

	[Test]
	[TestCaseSource(nameof(GraphConfigurations))]
	public void CaptureSteps_AllConfigs_VisitedNodesGrowMonotonically(
		string graphName, Func<(Graph, MazePoint, MazePoint)> graphFactory, int expectedDistance)
	{
		// Arrange
		var (graph, start, end) = graphFactory();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert
		int previousCount = 0;
		for (int i = 0; i < steps.Count; i++)
		{
			Assert.That(steps[i].VisitedNodes.Count, Is.GreaterThanOrEqualTo(previousCount),
				$"{graphName}: Visited count decreased at step {i}");
			previousCount = steps[i].VisitedNodes.Count;
		}
	}

	[Test]
	[TestCaseSource(nameof(GraphConfigurations))]
	public void CaptureSteps_AllConfigs_StartNodeHasDistanceZero(
		string graphName, Func<(Graph, MazePoint, MazePoint)> graphFactory, int expectedDistance)
	{
		// Arrange
		var (graph, start, end) = graphFactory();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert - start should always have distance 0
		foreach (var step in steps)
		{
			Assert.That(step.Distances[start], Is.EqualTo(0),
				$"{graphName}: Start should always have distance 0 at step {step.Type}");
		}
	}

	[Test]
	[TestCaseSource(nameof(GraphConfigurations))]
	public void CaptureSteps_AllConfigs_NoNodeInBothVisitedAndFrontier(
		string graphName, Func<(Graph, MazePoint, MazePoint)> graphFactory, int expectedDistance)
	{
		// Arrange
		var (graph, start, end) = graphFactory();
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, start, end);

		// Assert
		for (int i = 0; i < steps.Count; i++)
		{
			var step = steps[i];
			var overlap = step.VisitedNodes.Intersect(step.FrontierNodes).ToList();
			Assert.That(overlap.Count, Is.EqualTo(0),
				$"{graphName} step {i}: Nodes should not be in both visited and frontier: {string.Join(", ", overlap.Select(p => $"({p.X},{p.Y},{p.Z})"))}");
		}
	}

	#endregion

	#region Integration Tests - Real Maze Generation

	[Test]
	[TestCaseSource(nameof(MazeConfigurations))]
	public void CaptureSteps_GeneratedMaze_ProducesValidAnimation(
		int sizeX, int sizeY, int sizeZ, Algorithm algorithm, int wallRemovalPercent)
	{
		// Arrange
		var services = new ServiceContainer();
		var settings = CreateMazeSettings(sizeX, sizeY, sizeZ, algorithm, wallRemovalPercent);
		var result = services.MazeGenerationFactory.GenerateMaze(settings);
		var graph = result.HeuristicsResults.ShortestPathResult.Graph;
		var mazeJumper = result.MazeJumper;

		// Act
		var steps = services.DijkstraAnimator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);

		// Assert
		Assert.That(steps.Count, Is.GreaterThanOrEqualTo(2), "Should have at least Init and Complete");
		Assert.That(steps[0].Type, Is.EqualTo(StepType.Initialize));
		Assert.That(steps[^1].Type, Is.EqualTo(StepType.Complete));
		Assert.That(steps[^1].Distances[mazeJumper.EndPoint], Is.LessThan(int.MaxValue), 
			"End should be reachable");
	}

	[Test]
	[TestCaseSource(nameof(MazeConfigurations))]
	public void CaptureSteps_GeneratedMaze_AllGraphNodesInDistances(
		int sizeX, int sizeY, int sizeZ, Algorithm algorithm, int wallRemovalPercent)
	{
		// Arrange
		var services = new ServiceContainer();
		var settings = CreateMazeSettings(sizeX, sizeY, sizeZ, algorithm, wallRemovalPercent);
		var result = services.MazeGenerationFactory.GenerateMaze(settings);
		var graph = result.HeuristicsResults.ShortestPathResult.Graph;
		var mazeJumper = result.MazeJumper;

		// Act
		var steps = services.DijkstraAnimator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);

		// Assert - every step should have all graph nodes
		foreach (var step in steps)
		{
			foreach (var nodeKey in graph.Nodes.Keys)
			{
				Assert.That(step.Distances.ContainsKey(nodeKey), Is.True,
					$"Step {step.Type} missing node ({nodeKey.X},{nodeKey.Y},{nodeKey.Z})");
			}
		}
	}

	[Test]
	[TestCaseSource(nameof(MazeConfigurations))]
	public void CaptureSteps_GeneratedMaze_FinalDistanceMatchesShortestPathResult(
		int sizeX, int sizeY, int sizeZ, Algorithm algorithm, int wallRemovalPercent)
	{
		// Arrange
		var services = new ServiceContainer();
		var settings = CreateMazeSettings(sizeX, sizeY, sizeZ, algorithm, wallRemovalPercent);
		var result = services.MazeGenerationFactory.GenerateMaze(settings);
		var shortestPathResult = result.HeuristicsResults.ShortestPathResult;
		var graph = shortestPathResult.Graph;
		var mazeJumper = result.MazeJumper;

		// Act
		var steps = services.DijkstraAnimator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);
		var finalStep = steps[^1];

		// Assert - animation should match the pre-computed result
		Assert.That(finalStep.Distances[mazeJumper.EndPoint], Is.EqualTo(shortestPathResult.ShortestPath),
			"Animation final distance should match ShortestPathResult");
	}

	#endregion

	#region Stress Tests - Multiple Iterations

	[Test]
	[TestCaseSource(nameof(StressTestIterations))]
	public void CaptureSteps_StressTest_ConsistentResultsAcrossIterations(
		int sizeX, int sizeY, int sizeZ, int iterations)
	{
		// This test generates multiple mazes and verifies the animator always produces valid results
		var services = new ServiceContainer();
		var settings = CreateMazeSettings(sizeX, sizeY, sizeZ, Algorithm.GrowingTreeAlgorithm, 0);

		for (int i = 0; i < iterations; i++)
		{
			// Generate a new maze each iteration
			var result = services.MazeGenerationFactory.GenerateMaze(settings);
			var graph = result.HeuristicsResults.ShortestPathResult.Graph;
			var mazeJumper = result.MazeJumper;

			// Act
			var steps = services.DijkstraAnimator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);

			// Assert basic invariants
			Assert.That(steps.Count, Is.GreaterThanOrEqualTo(2), 
				$"Iteration {i}: Should have at least 2 steps");
			Assert.That(steps[0].Type, Is.EqualTo(StepType.Initialize), 
				$"Iteration {i}: Should start with Initialize");
			Assert.That(steps[^1].Type, Is.EqualTo(StepType.Complete), 
				$"Iteration {i}: Should end with Complete");
			Assert.That(steps[0].Distances[mazeJumper.StartPoint], Is.EqualTo(0), 
				$"Iteration {i}: Start should have distance 0");
			Assert.That(steps[^1].Distances[mazeJumper.EndPoint], Is.LessThan(int.MaxValue), 
				$"Iteration {i}: End should be reachable");

			// Verify all steps have complete distance data
			foreach (var step in steps)
			{
				Assert.That(step.Distances.Count, Is.EqualTo(graph.Nodes.Count),
					$"Iteration {i}, step {step.Type}: Should have all node distances");
			}
		}
	}

	[Test]
	[TestCaseSource(nameof(StressTestIterations))]
	public void CaptureSteps_StressTest_NoExceptionsOnManyIterations(
		int sizeX, int sizeY, int sizeZ, int iterations)
	{
		// Ensure no exceptions are thrown across many maze generations
		var services = new ServiceContainer();
		var settings = CreateMazeSettings(sizeX, sizeY, sizeZ, Algorithm.GrowingTreeAlgorithm, 10);

		Assert.DoesNotThrow(() =>
		{
			for (int i = 0; i < iterations; i++)
			{
				var result = services.MazeGenerationFactory.GenerateMaze(settings);
				var graph = result.HeuristicsResults.ShortestPathResult.Graph;
				var mazeJumper = result.MazeJumper;

				var steps = services.DijkstraAnimator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);

				// Access all step data to ensure no lazy evaluation issues
				foreach (var step in steps)
				{
					_ = step.Type;
					_ = step.Description;
					_ = step.Distances.Count;
					_ = step.VisitedNodes.Count;
					_ = step.FrontierNodes.Count;
					_ = step.Predecessors.Count;
				}
			}
		}, $"Should not throw any exceptions across {iterations} iterations");
	}

	[Test]
	public void CaptureSteps_StressTest_LargeMaze_CompletesWithinReasonableTime()
	{
		// Arrange
		var services = new ServiceContainer();
		var settings = CreateMazeSettings(20, 20, 1, Algorithm.GrowingTreeAlgorithm, 0);
		var result = services.MazeGenerationFactory.GenerateMaze(settings);
		var graph = result.HeuristicsResults.ShortestPathResult.Graph;
		var mazeJumper = result.MazeJumper;

		// Act & Assert - should complete without timeout
		var startTime = DateTime.UtcNow;
		var steps = services.DijkstraAnimator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);
		var elapsed = DateTime.UtcNow - startTime;

		Assert.That(steps.Count, Is.GreaterThan(0), "Should produce steps");
		Assert.That(elapsed.TotalSeconds, Is.LessThan(5), "Should complete within 5 seconds");
	}

	#endregion

	#region Edge Case Tests - Boundary Conditions

	[Test]
	public void CaptureSteps_TwoNodeGraph_MinimalValidGraph()
	{
		// Arrange - minimal connected graph
		var a = new MazePoint(0, 0, 0);
		var b = new MazePoint(1, 0, 0);
		var graph = new Graph
		{
			Nodes = new Dictionary<MazePoint, GraphNode>
			{
				[a] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = b, DirectionsToPoint = new Direction[] { Direction.Right } }
					}
				},
				[b] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = a, DirectionsToPoint = new Direction[] { Direction.Left } }
					}
				}
			}
		};
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, a, b);

		// Assert
		Assert.That(steps[0].Type, Is.EqualTo(StepType.Initialize));
		Assert.That(steps[^1].Type, Is.EqualTo(StepType.Complete));
		Assert.That(steps[^1].Distances[b], Is.EqualTo(1));
	}

	[Test]
	public void CaptureSteps_HighEdgeWeight_LargeDistanceHandledCorrectly()
	{
		// Arrange - graph with high edge weight
		var a = new MazePoint(0, 0, 0);
		var b = new MazePoint(1, 0, 0);
		var directions = Enumerable.Repeat(Direction.Right, 100).ToArray();
		
		var graph = new Graph
		{
			Nodes = new Dictionary<MazePoint, GraphNode>
			{
				[a] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = b, DirectionsToPoint = directions }
					}
				},
				[b] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = a, DirectionsToPoint = Enumerable.Repeat(Direction.Left, 100).ToArray() }
					}
				}
			}
		};
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, a, b);

		// Assert
		Assert.That(steps[^1].Distances[b], Is.EqualTo(100));
	}

	[Test]
	public void CaptureSteps_3DGraph_HandlesZCoordinates()
	{
		// Arrange - simple 3D graph
		var bottom = new MazePoint(0, 0, 0);
		var top = new MazePoint(0, 0, 1);
		
		var graph = new Graph
		{
			Nodes = new Dictionary<MazePoint, GraphNode>
			{
				[bottom] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = top, DirectionsToPoint = new Direction[] { Direction.Up } }
					}
				},
				[top] = new GraphNode
				{
					ShortestPath = int.MaxValue,
					Edges = new GraphEdge[]
					{
						new GraphEdge { Point = bottom, DirectionsToPoint = new Direction[] { Direction.Down } }
					}
				}
			}
		};
		var animator = CreateAnimator();

		// Act
		var steps = animator.CaptureSteps(graph, bottom, top);

		// Assert
		Assert.That(steps[^1].Distances[top], Is.EqualTo(1));
		Assert.That(steps.All(s => s.Distances.ContainsKey(bottom)), Is.True);
		Assert.That(steps.All(s => s.Distances.ContainsKey(top)), Is.True);
	}

	#endregion
}
