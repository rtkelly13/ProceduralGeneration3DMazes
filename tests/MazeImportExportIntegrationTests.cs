using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Serialization;

namespace ProceduralMaze.Tests;

/// <summary>
/// Integration tests for the import/export pipeline.
/// Tests the flow: IModel -> IMazeJumper -> ShortestPathSolver
/// </summary>
[TestFixture]
public class MazeImportExportIntegrationTests
{
    private ServiceContainer _services = null!;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceContainer();
    }

    #region GetMazeJumperFromModel Tests

    [Test]
    public void GetMazeJumperFromModel_ValidModel_CreatesJumperWithCorrectProperties()
    {
        // Arrange
        var mazeText = @"
SIZE 3 3 1
START 0 0 0
END 2 2 0
CELLS
0 0 0 RF
1 0 0 LRF
2 0 0 LF
0 1 0 BRF
1 1 0 LRBF
2 1 0 LBF
0 2 0 BR
1 2 0 LRB
2 2 0 LB
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Act
        var jumper = _services.MazeFactory.GetMazeJumperFromModel(model);

        // Assert
        Assert.That(jumper.Size.X, Is.EqualTo(3));
        Assert.That(jumper.Size.Y, Is.EqualTo(3));
        Assert.That(jumper.Size.Z, Is.EqualTo(1));
        Assert.That(jumper.StartPoint, Is.EqualTo(new MazePoint(0, 0, 0)));
        Assert.That(jumper.EndPoint, Is.EqualTo(new MazePoint(2, 2, 0)));
        Assert.That(jumper.CurrentPoint, Is.EqualTo(new MazePoint(0, 0, 0)));
    }

    [Test]
    public void GetMazeJumperFromModel_CanNavigateMaze()
    {
        // Arrange
        var mazeText = @"
SIZE 3 3 1
START 0 0 0
END 2 2 0
CELLS
0 0 0 RF
1 0 0 LRF
2 0 0 LF
0 1 0 BRF
1 1 0 LRBF
2 1 0 LBF
0 2 0 BR
1 2 0 LRB
2 2 0 LB
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);
        var jumper = _services.MazeFactory.GetMazeJumperFromModel(model);

        // Act - navigate through the maze
        jumper.JumpToPoint(new MazePoint(1, 1, 0));

        // Assert - can check directions at center cell
        var directions = jumper.GetFlagFromPoint();
        Assert.That(directions, Is.EqualTo(Direction.Left | Direction.Right | Direction.Back | Direction.Forward));
    }

    [Test]
    public void GetMazeJumperFromModel_3DMaze_PreservesVerticalConnections()
    {
        // Arrange
        var mazeText = @"
SIZE 2 2 2
START 0 0 0
END 1 1 1
CELLS
0 0 0 RFU
1 0 0 LFU
0 1 0 BRU
1 1 0 BLU
0 0 1 RFD
1 0 1 LFD
0 1 1 BRD
1 1 1 BLD
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Act
        var jumper = _services.MazeFactory.GetMazeJumperFromModel(model);

        // Assert - check vertical connections
        jumper.JumpToPoint(new MazePoint(0, 0, 0));
        Assert.That(jumper.GetFlagFromPoint().HasFlag(Direction.Up), Is.True);

        jumper.JumpToPoint(new MazePoint(0, 0, 1));
        Assert.That(jumper.GetFlagFromPoint().HasFlag(Direction.Down), Is.True);
    }

    #endregion

    #region ReadOnlyModelsWrapper Tests

    [Test]
    public void ReadOnlyModelsWrapper_SetState_SwitchesBetweenModes()
    {
        // Arrange
        var mazeText = @"
SIZE 3 3 1
START 0 0 0
END 2 2 0
CELLS
0 0 0 RF
1 0 0 LRF
2 0 0 LF
0 1 0 BRF
1 1 0 LRBF
2 1 0 LBF
0 2 0 BR
1 2 0 LRB
2 2 0 LB
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);
        var wrapper = new ReadOnlyModelsWrapper(model);

        // Act & Assert - starts in standard mode
        Assert.That(wrapper.ModelMode, Is.EqualTo(ModelMode.Standard));
        Assert.That(wrapper.DeadEnded, Is.False);

        // Act & Assert - can set up dead end wrapping
        wrapper.DoDeadEndWrapping(mb => _services.DeadEndModelWrapperFactory.MakeModel(mb));
        wrapper.SetState(ModelMode.DeadEndFilled);
        Assert.That(wrapper.ModelMode, Is.EqualTo(ModelMode.DeadEndFilled));
        Assert.That(wrapper.DeadEnded, Is.True);

        // Act & Assert - can switch back to standard
        wrapper.SetState(ModelMode.Standard);
        Assert.That(wrapper.ModelMode, Is.EqualTo(ModelMode.Standard));
        Assert.That(wrapper.DeadEnded, Is.False);
    }

    [Test]
    public void ReadOnlyModelsWrapper_SetDeadEndWithoutWrapping_ThrowsInvalidOperationException()
    {
        // Arrange
        var mazeText = @"
SIZE 2 2 1
START 0 0 0
END 1 1 0
CELLS
0 0 0 RF
1 0 0 LF
0 1 0 BR
1 1 0 BL
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);
        var wrapper = new ReadOnlyModelsWrapper(model);

        // Act & Assert - trying to set dead-end mode without wrapping throws
        Assert.Throws<System.InvalidOperationException>(() => wrapper.SetState(ModelMode.DeadEndFilled));
    }

    #endregion

    #region IMazeJumper.GetModel() Tests

    [Test]
    public void MazeJumper_GetModel_ReturnsValidModel()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 5, Y = 5, Z = 1 },
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
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);
        var jumper = result.MazeJumper;

        // Act
        var model = jumper.GetModel();

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Size.X, Is.EqualTo(5));
        Assert.That(model.Size.Y, Is.EqualTo(5));
        Assert.That(model.Size.Z, Is.EqualTo(1));
        Assert.That(model.StartPoint, Is.EqualTo(jumper.StartPoint));
        Assert.That(model.EndPoint, Is.EqualTo(jumper.EndPoint));
    }

    [Test]
    public void MazeJumper_GetModel_PreservesCellDirections()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 5, Y = 5, Z = 1 },
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
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);
        var jumper = result.MazeJumper;

        // Act
        var model = jumper.GetModel();

        // Assert - verify all cells match
        for (int y = 0; y < jumper.Size.Y; y++)
        {
            for (int x = 0; x < jumper.Size.X; x++)
            {
                var point = new MazePoint(x, y, 0);
                jumper.JumpToPoint(point);
                var jumperDirs = jumper.GetFlagFromPoint();
                var modelDirs = model.GetFlagFromPoint(point);
                Assert.That(modelDirs, Is.EqualTo(jumperDirs),
                    $"Cell ({x}, {y}, 0) mismatch");
            }
        }
    }

    #endregion

    #region Round-Trip Tests

    [Test]
    public void RoundTrip_GenerateThenImport_PreservesMazeData()
    {
        // Arrange - generate a maze
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
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);
        var originalJumper = result.MazeJumper;

        // Act - serialize and deserialize
        var originalModel = originalJumper.GetModel();
        var serialized = _services.MazeSerializer.SerializeToString(originalModel);
        var importedModel = _services.MazeDeserializer.DeserializeFromString(serialized);
        var importedJumper = _services.MazeFactory.GetMazeJumperFromModel(importedModel);

        // Assert - compare all cells
        Assert.That(importedJumper.Size.X, Is.EqualTo(originalJumper.Size.X));
        Assert.That(importedJumper.Size.Y, Is.EqualTo(originalJumper.Size.Y));
        Assert.That(importedJumper.Size.Z, Is.EqualTo(originalJumper.Size.Z));
        Assert.That(importedJumper.StartPoint, Is.EqualTo(originalJumper.StartPoint));
        Assert.That(importedJumper.EndPoint, Is.EqualTo(originalJumper.EndPoint));

        for (int z = 0; z < originalJumper.Size.Z; z++)
        {
            for (int y = 0; y < originalJumper.Size.Y; y++)
            {
                for (int x = 0; x < originalJumper.Size.X; x++)
                {
                    var point = new MazePoint(x, y, z);
                    originalJumper.JumpToPoint(point);
                    importedJumper.JumpToPoint(point);
                    Assert.That(importedJumper.GetFlagFromPoint(), Is.EqualTo(originalJumper.GetFlagFromPoint()),
                        $"Cell ({x}, {y}, {z}) mismatch after round-trip");
                }
            }
        }
    }

    [Test]
    public void RoundTrip_3DMaze_PreservesAllConnections()
    {
        // Arrange - generate a 3D maze
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 5, Y = 5, Z = 3 },
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
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);
        var originalJumper = result.MazeJumper;

        // Act - round-trip
        var originalModel = originalJumper.GetModel();
        var serialized = _services.MazeSerializer.SerializeToString(originalModel);
        var importedModel = _services.MazeDeserializer.DeserializeFromString(serialized);
        var importedJumper = _services.MazeFactory.GetMazeJumperFromModel(importedModel);

        // Assert - verify all cells including vertical connections
        for (int z = 0; z < originalJumper.Size.Z; z++)
        {
            for (int y = 0; y < originalJumper.Size.Y; y++)
            {
                for (int x = 0; x < originalJumper.Size.X; x++)
                {
                    var point = new MazePoint(x, y, z);
                    originalJumper.JumpToPoint(point);
                    importedJumper.JumpToPoint(point);
                    Assert.That(importedJumper.GetFlagFromPoint(), Is.EqualTo(originalJumper.GetFlagFromPoint()),
                        $"Cell ({x}, {y}, {z}) mismatch after 3D round-trip");
                }
            }
        }
    }

    #endregion

    #region ShortestPathSolver with Imported Maze Tests

    [Test]
    public void ShortestPathSolver_ImportedMaze_ComputesValidPath()
    {
        // Arrange
        var mazeText = @"
SIZE 3 3 1
START 0 0 0
END 2 2 0
CELLS
0 0 0 RF
1 0 0 LRF
2 0 0 LF
0 1 0 BRF
1 1 0 LRBF
2 1 0 LBF
0 2 0 BR
1 2 0 LRB
2 2 0 LB
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);
        var jumper = _services.MazeFactory.GetMazeJumperFromModel(model);

        // Act
        var result = _services.ShortestPathSolver.GetGraph(jumper);

        // Assert
        Assert.That(result.ShortestPath, Is.GreaterThan(0));
        Assert.That(result.ShortestPathDirections, Is.Not.Empty);
        Assert.That(result.Graph, Is.Not.Null);
        Assert.That(result.Graph.Nodes, Is.Not.Empty);
    }

    [Test]
    public void ShortestPathSolver_ImportedMaze_PathReachesEnd()
    {
        // Arrange
        var mazeText = @"
SIZE 3 3 1
START 0 0 0
END 2 2 0
CELLS
0 0 0 RF
1 0 0 LRF
2 0 0 LF
0 1 0 BRF
1 1 0 LRBF
2 1 0 LBF
0 2 0 BR
1 2 0 LRB
2 2 0 LB
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);
        var jumper = _services.MazeFactory.GetMazeJumperFromModel(model);

        // Act
        var result = _services.ShortestPathSolver.GetGraph(jumper);

        // Follow the path from start
        var currentPoint = jumper.StartPoint;
        foreach (var direction in result.ShortestPathDirections)
        {
            currentPoint = _services.MovementHelper.Move(currentPoint, direction, jumper.Size);
        }

        // Assert
        Assert.That(currentPoint, Is.EqualTo(jumper.EndPoint));
    }

    [Test]
    public void ShortestPathSolver_ImportedMaze_MatchesGeneratedMaze()
    {
        // Arrange - generate a maze and compute path
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
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);
        var originalJumper = result.MazeJumper;
        var originalPath = _services.ShortestPathSolver.GetGraph(originalJumper);

        // Act - round-trip and compute path again
        var model = originalJumper.GetModel();
        var serialized = _services.MazeSerializer.SerializeToString(model);
        var importedModel = _services.MazeDeserializer.DeserializeFromString(serialized);
        var importedJumper = _services.MazeFactory.GetMazeJumperFromModel(importedModel);
        var importedPath = _services.ShortestPathSolver.GetGraph(importedJumper);

        // Assert - paths should be identical
        Assert.That(importedPath.ShortestPath, Is.EqualTo(originalPath.ShortestPath));
        Assert.That(importedPath.ShortestPathDirections, Is.EqualTo(originalPath.ShortestPathDirections));
    }

    #endregion

    #region Edge Cases

    [Test]
    public void GetMazeJumperFromModel_1x1Maze_Works()
    {
        // Arrange
        var mazeText = @"
SIZE 1 1 1
START 0 0 0
END 0 0 0
CELLS
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Act
        var jumper = _services.MazeFactory.GetMazeJumperFromModel(model);

        // Assert
        Assert.That(jumper.Size.X, Is.EqualTo(1));
        Assert.That(jumper.Size.Y, Is.EqualTo(1));
        Assert.That(jumper.Size.Z, Is.EqualTo(1));
        Assert.That(jumper.StartPoint, Is.EqualTo(jumper.EndPoint));
        Assert.That(jumper.GetFlagFromPoint(), Is.EqualTo(Direction.None));
    }

    [Test]
    public void GetMazeJumperFromModel_LinearMaze_Works()
    {
        // Arrange - a simple linear maze: 0 <-> 1 <-> 2
        var mazeText = @"
SIZE 3 1 1
START 0 0 0
END 2 0 0
CELLS
0 0 0 R
1 0 0 LR
2 0 0 L
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Act
        var jumper = _services.MazeFactory.GetMazeJumperFromModel(model);
        var pathResult = _services.ShortestPathSolver.GetGraph(jumper);

        // Assert
        Assert.That(pathResult.ShortestPath, Is.EqualTo(2));
        Assert.That(pathResult.ShortestPathDirections, Is.EqualTo(new[] { Direction.Right, Direction.Right }));
    }

    [Test]
    public void ImportedMaze_WithWallRemoval_PreservesExtraConnections()
    {
        // Arrange - maze with extra connections (not a perfect maze)
        var mazeText = @"
SIZE 2 2 1
START 0 0 0
END 1 1 0
CELLS
0 0 0 RF
1 0 0 LF
0 1 0 BRF
1 1 0 BLB
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Act
        var jumper = _services.MazeFactory.GetMazeJumperFromModel(model);

        // Assert - verify the connections are preserved
        jumper.JumpToPoint(new MazePoint(0, 1, 0));
        var dirs = jumper.GetFlagFromPoint();
        Assert.That(dirs.HasFlag(Direction.Forward), Is.True, "Should have forward connection");
        Assert.That(dirs.HasFlag(Direction.Back), Is.True, "Should have back connection");
        Assert.That(dirs.HasFlag(Direction.Right), Is.True, "Should have right connection");
    }

    #endregion
}
