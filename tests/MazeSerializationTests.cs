using System.IO;
using System.Text.Json;
using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Serialization;

namespace ProceduralMaze.Tests;

[TestFixture]
public class MazeSerializationTests
{
    private ServiceContainer _services = null!;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [SetUp]
    public void Setup()
    {
        _services = new ServiceContainer();
    }

    #region MazeSerializer Tests

    [Test]
    public void Serialize_SimpleMaze_ProducesExpectedFormat()
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

        // Act
        var serialized = _services.MazeSerializer.SerializeToString(model);

        // Assert
        Assert.That(serialized, Does.Contain("SIZE 2 2 1"));
        Assert.That(serialized, Does.Contain("START 0 0 0"));
        Assert.That(serialized, Does.Contain("END 1 1 0"));
        Assert.That(serialized, Does.Contain("CELLS"));
        // Direction order in output is L,R,D,U,B,F - so RF stays RF, BR becomes RB, BL becomes LB
        Assert.That(serialized, Does.Contain("0 0 0 RF"));
        Assert.That(serialized, Does.Contain("1 0 0 LF"));
        Assert.That(serialized, Does.Contain("0 1 0 RB"));
        Assert.That(serialized, Does.Contain("1 1 0 LB"));
    }

    [Test]
    public void Serialize_ToStream_WritesCorrectData()
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
        using var stream = new MemoryStream();

        // Act
        _services.MazeSerializer.Serialize(model, stream);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var serialized = reader.ReadToEnd();

        // Assert
        Assert.That(serialized, Does.Contain("SIZE 2 2 1"));
        Assert.That(serialized, Does.Contain("CELLS"));
    }

    [Test]
    public void Serialize_OmitsEmptyCells()
    {
        // Arrange - a maze where only some cells have connections
        var mazeText = @"
SIZE 3 3 1
START 0 0 0
END 2 2 0
CELLS
0 0 0 R
1 0 0 L
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Act
        var serialized = _services.MazeSerializer.SerializeToString(model);

        // Assert - should not contain cell data lines for cells without connections
        // Note: "2 2 0" appears in "END 2 2 0", so we check for the cell pattern specifically
        var lines = serialized.Split('\n');
        var cellLines = lines.Where(l => l.Trim().StartsWith("2 2 0") || l.Trim().StartsWith("1 1 0")).ToList();
        Assert.That(cellLines, Is.Empty, "Should not have cell data lines for empty cells");
    }

    [Test]
    public void RoundTrip_GeneratedMaze_PreservesAllCells()
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
        var mazeJumper = result.MazeJumper;

        // Build a model from the maze jumper for serialization
        var originalModel = BuildModelFromMazeJumper(mazeJumper);

        // Act
        var serialized = _services.MazeSerializer.SerializeToString(originalModel);
        var deserializedModel = _services.MazeDeserializer.DeserializeFromString(serialized);

        // Assert - compare all cells
        Assert.That(deserializedModel.Size.X, Is.EqualTo(originalModel.Size.X));
        Assert.That(deserializedModel.Size.Y, Is.EqualTo(originalModel.Size.Y));
        Assert.That(deserializedModel.Size.Z, Is.EqualTo(originalModel.Size.Z));
        Assert.That(deserializedModel.StartPoint, Is.EqualTo(originalModel.StartPoint));
        Assert.That(deserializedModel.EndPoint, Is.EqualTo(originalModel.EndPoint));

        for (int z = 0; z < originalModel.Size.Z; z++)
        {
            for (int y = 0; y < originalModel.Size.Y; y++)
            {
                for (int x = 0; x < originalModel.Size.X; x++)
                {
                    var point = new MazePoint(x, y, z);
                    var originalDirs = originalModel.GetFlagFromPoint(point);
                    var deserializedDirs = deserializedModel.GetFlagFromPoint(point);
                    Assert.That(deserializedDirs, Is.EqualTo(originalDirs),
                        $"Cell ({x}, {y}, {z}) mismatch: expected {originalDirs}, got {deserializedDirs}");
                }
            }
        }
    }

    private ReadOnlyMazeModel BuildModelFromMazeJumper(IMazeJumper mazeJumper)
    {
        var cells = new System.Collections.Generic.Dictionary<MazePoint, Direction>();
        for (int z = 0; z < mazeJumper.Size.Z; z++)
        {
            for (int y = 0; y < mazeJumper.Size.Y; y++)
            {
                for (int x = 0; x < mazeJumper.Size.X; x++)
                {
                    var point = new MazePoint(x, y, z);
                    mazeJumper.JumpToPoint(point);
                    var dirs = mazeJumper.GetFlagFromPoint();
                    if (dirs != Direction.None)
                    {
                        cells[point] = dirs;
                    }
                }
            }
        }
        return new ReadOnlyMazeModel(mazeJumper.Size, mazeJumper.StartPoint, mazeJumper.EndPoint, cells);
    }

    [Test]
    public void RoundTrip_3DMaze_PreservesVerticalConnections()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 3, Y = 3, Z = 3 },
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
        var mazeJumper = result.MazeJumper;
        var originalModel = BuildModelFromMazeJumper(mazeJumper);

        // Act
        var serialized = _services.MazeSerializer.SerializeToString(originalModel);
        var deserializedModel = _services.MazeDeserializer.DeserializeFromString(serialized);

        // Assert - verify all cells including vertical connections
        for (int z = 0; z < originalModel.Size.Z; z++)
        {
            for (int y = 0; y < originalModel.Size.Y; y++)
            {
                for (int x = 0; x < originalModel.Size.X; x++)
                {
                    var point = new MazePoint(x, y, z);
                    var originalDirs = originalModel.GetFlagFromPoint(point);
                    var deserializedDirs = deserializedModel.GetFlagFromPoint(point);
                    Assert.That(deserializedDirs, Is.EqualTo(originalDirs),
                        $"Cell ({x}, {y}, {z}) mismatch");
                }
            }
        }
    }

    #endregion

    #region MazeDeserializer Tests

    [Test]
    public void Deserialize_ValidMaze_ParsesCorrectly()
    {
        // Arrange
        var mazeText = @"
# This is a comment
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

        // Act
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Assert
        Assert.That(model.Size.X, Is.EqualTo(3));
        Assert.That(model.Size.Y, Is.EqualTo(3));
        Assert.That(model.Size.Z, Is.EqualTo(1));
        Assert.That(model.StartPoint, Is.EqualTo(new MazePoint(0, 0, 0)));
        Assert.That(model.EndPoint, Is.EqualTo(new MazePoint(2, 2, 0)));

        // Check specific cells
        Assert.That(model.GetFlagFromPoint(new MazePoint(0, 0, 0)), Is.EqualTo(Direction.Right | Direction.Forward));
        Assert.That(model.GetFlagFromPoint(new MazePoint(1, 1, 0)), Is.EqualTo(Direction.Left | Direction.Right | Direction.Back | Direction.Forward));
        Assert.That(model.GetFlagFromPoint(new MazePoint(2, 2, 0)), Is.EqualTo(Direction.Left | Direction.Back));
    }

    [Test]
    public void Deserialize_NumericDirections_ParsesCorrectly()
    {
        // Arrange - using numeric direction values
        // RF = Right(2) | Forward(32) = 34
        var mazeText = @"
SIZE 2 2 1
START 0 0 0
END 1 1 0
CELLS
0 0 0 34
1 0 0 33
0 1 0 18
1 1 0 17
";

        // Act
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Assert
        Assert.That(model.GetFlagFromPoint(new MazePoint(0, 0, 0)), Is.EqualTo(Direction.Right | Direction.Forward));
        Assert.That(model.GetFlagFromPoint(new MazePoint(1, 0, 0)), Is.EqualTo(Direction.Left | Direction.Forward));
        Assert.That(model.GetFlagFromPoint(new MazePoint(0, 1, 0)), Is.EqualTo(Direction.Right | Direction.Back));
        Assert.That(model.GetFlagFromPoint(new MazePoint(1, 1, 0)), Is.EqualTo(Direction.Left | Direction.Back));
    }

    [Test]
    public void Deserialize_MixedCaseDirections_ParsesCorrectly()
    {
        // Arrange
        var mazeText = @"
SIZE 2 1 1
START 0 0 0
END 1 0 0
CELLS
0 0 0 r
1 0 0 L
";

        // Act
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Assert
        Assert.That(model.GetFlagFromPoint(new MazePoint(0, 0, 0)), Is.EqualTo(Direction.Right));
        Assert.That(model.GetFlagFromPoint(new MazePoint(1, 0, 0)), Is.EqualTo(Direction.Left));
    }

    [Test]
    public void Deserialize_OmittedCells_DefaultToNone()
    {
        // Arrange - only define some cells
        var mazeText = @"
SIZE 3 3 1
START 0 0 0
END 2 2 0
CELLS
0 0 0 R
1 0 0 L
";

        // Act
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Assert
        Assert.That(model.GetFlagFromPoint(new MazePoint(0, 0, 0)), Is.EqualTo(Direction.Right));
        Assert.That(model.GetFlagFromPoint(new MazePoint(1, 0, 0)), Is.EqualTo(Direction.Left));
        Assert.That(model.GetFlagFromPoint(new MazePoint(2, 2, 0)), Is.EqualTo(Direction.None));
        Assert.That(model.GetFlagFromPoint(new MazePoint(1, 1, 0)), Is.EqualTo(Direction.None));
    }

    [Test]
    public void Deserialize_CommentsAndBlankLines_AreIgnored()
    {
        // Arrange
        var mazeText = @"
# Header comment
SIZE 2 1 1

# Another comment
START 0 0 0

END 1 0 0

# Before cells
CELLS
# Cell comment
0 0 0 R
1 0 0 L
# End comment
";

        // Act
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Assert
        Assert.That(model.Size.X, Is.EqualTo(2));
        Assert.That(model.GetFlagFromPoint(new MazePoint(0, 0, 0)), Is.EqualTo(Direction.Right));
    }

    [Test]
    public void Deserialize_FromStream_ParsesCorrectly()
    {
        // Arrange
        var mazeText = @"
SIZE 2 1 1
START 0 0 0
END 1 0 0
CELLS
0 0 0 R
1 0 0 L
";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(mazeText));

        // Act
        var model = _services.MazeDeserializer.Deserialize(stream);

        // Assert
        Assert.That(model.Size.X, Is.EqualTo(2));
        Assert.That(model.GetFlagFromPoint(new MazePoint(0, 0, 0)), Is.EqualTo(Direction.Right));
    }

    [Test]
    public void Deserialize_MissingSize_ThrowsFormatException()
    {
        // Arrange
        var mazeText = @"
START 0 0 0
END 1 0 0
CELLS
0 0 0 R
";

        // Act & Assert
        Assert.Throws<System.FormatException>(() => _services.MazeDeserializer.DeserializeFromString(mazeText));
    }

    [Test]
    public void Deserialize_MissingStart_ThrowsFormatException()
    {
        // Arrange
        var mazeText = @"
SIZE 2 1 1
END 1 0 0
CELLS
0 0 0 R
";

        // Act & Assert
        Assert.Throws<System.FormatException>(() => _services.MazeDeserializer.DeserializeFromString(mazeText));
    }

    [Test]
    public void Deserialize_MissingEnd_ThrowsFormatException()
    {
        // Arrange
        var mazeText = @"
SIZE 2 1 1
START 0 0 0
CELLS
0 0 0 R
";

        // Act & Assert
        Assert.Throws<System.FormatException>(() => _services.MazeDeserializer.DeserializeFromString(mazeText));
    }

    [Test]
    public void Deserialize_InvalidDirectionChar_ThrowsFormatException()
    {
        // Arrange
        var mazeText = @"
SIZE 2 1 1
START 0 0 0
END 1 0 0
CELLS
0 0 0 RX
";

        // Act & Assert
        Assert.Throws<System.FormatException>(() => _services.MazeDeserializer.DeserializeFromString(mazeText));
    }

    [Test]
    public void Deserialize_InvalidNumericDirection_ThrowsFormatException()
    {
        // Arrange
        var mazeText = @"
SIZE 2 1 1
START 0 0 0
END 1 0 0
CELLS
0 0 0 100
";

        // Act & Assert
        Assert.Throws<System.FormatException>(() => _services.MazeDeserializer.DeserializeFromString(mazeText));
    }

    #endregion

    #region MazeValidator Tests

    [Test]
    public void Validate_ValidMaze_ReturnsNoErrors()
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

        // Act
        var result = _services.MazeValidator.Validate(model);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Validate_AsymmetricConnection_ReturnsError()
    {
        // Arrange - cell (0,0,0) points Right but (1,0,0) doesn't point Left
        var mazeText = @"
SIZE 2 1 1
START 0 0 0
END 1 0 0
CELLS
0 0 0 R
1 0 0 N
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Act
        var result = _services.MazeValidator.Validate(model);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("Asymmetric"));
    }

    [Test]
    public void Validate_BoundaryViolation_ReturnsError()
    {
        // Arrange - cell at edge points out of bounds
        var mazeText = @"
SIZE 2 1 1
START 0 0 0
END 1 0 0
CELLS
0 0 0 LR
1 0 0 LR
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Act
        var result = _services.MazeValidator.Validate(model);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("outside maze bounds"));
    }

    [Test]
    public void Validate_IsolatedStartPoint_ReturnsError()
    {
        // Arrange - start point has no connections
        var mazeText = @"
SIZE 2 2 1
START 0 0 0
END 1 1 0
CELLS
1 0 0 F
1 1 0 B
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Act
        var result = _services.MazeValidator.Validate(model);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("Start point").And.Some.Contains("isolated"));
    }

    [Test]
    public void Validate_IsolatedEndPoint_ReturnsError()
    {
        // Arrange - end point has no connections
        var mazeText = @"
SIZE 2 2 1
START 0 0 0
END 1 1 0
CELLS
0 0 0 RF
1 0 0 LF
0 1 0 BR
";
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Act
        var result = _services.MazeValidator.Validate(model);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Contains("End point").And.Some.Contains("isolated"));
    }

    [Test]
    public void Validate_GeneratedMaze_IsValid()
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
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);
        var mazeJumper = result.MazeJumper;
        var model = BuildModelFromMazeJumper(mazeJumper);

        // Act
        var validation = _services.MazeValidator.Validate(model);

        // Assert
        Assert.That(validation.IsValid, Is.True, string.Join(", ", validation.Errors));
    }

    #endregion

    #region Integration Tests

    [Test]
    public void DeserializedMaze_CanBeValidated()
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
        var validationResult = _services.MazeValidator.Validate(model);

        // Assert
        Assert.That(validationResult.IsValid, Is.True);
        Assert.That(validationResult.Errors, Is.Empty);
    }

    [Test]
    public void DeserializedMaze_1x1x1_HandlesEdgeCase()
    {
        // Arrange
        var mazeText = @"
SIZE 1 1 1
START 0 0 0
END 0 0 0
CELLS
";

        // Act
        var model = _services.MazeDeserializer.DeserializeFromString(mazeText);

        // Assert
        Assert.That(model.Size.X, Is.EqualTo(1));
        Assert.That(model.Size.Y, Is.EqualTo(1));
        Assert.That(model.Size.Z, Is.EqualTo(1));
        Assert.That(model.GetFlagFromPoint(new MazePoint(0, 0, 0)), Is.EqualTo(Direction.None));
    }

    #endregion

    #region MazeStatsSerializer Tests

    [Test]
    public void SerializeStats_ValidMaze_ProducesValidJson()
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

        // Act
        var statsOutput = _services.MazeStatsSerializer.SerializeToString(result);

        // Assert - verify it's valid JSON by deserializing
        var data = JsonSerializer.Deserialize<MazeStatsData>(statsOutput, JsonOptions);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Dimensions, Is.Not.Null);
        Assert.That(data.Endpoints, Is.Not.Null);
        Assert.That(data.Path, Is.Not.Null);
        Assert.That(data.DeadEnds, Is.Not.Null);
        Assert.That(data.DirectionUsage, Is.Not.Null);
        Assert.That(data.Timing, Is.Not.Null);
    }

    [Test]
    public void SerializeStats_ValidMaze_ContainsDimensionData()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 10, Y = 8, Z = 2 },
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

        // Act
        var statsOutput = _services.MazeStatsSerializer.SerializeToString(result);
        var data = JsonSerializer.Deserialize<MazeStatsData>(statsOutput, JsonOptions);

        // Assert
        Assert.That(data!.Dimensions.Width, Is.EqualTo(10));
        Assert.That(data.Dimensions.Height, Is.EqualTo(8));
        Assert.That(data.Dimensions.Depth, Is.EqualTo(2));
        Assert.That(data.Dimensions.TotalCells, Is.EqualTo(160)); // 10*8*2
    }

    [Test]
    public void SerializeStats_ValidMaze_ContainsPathInfo()
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

        // Act
        var statsOutput = _services.MazeStatsSerializer.SerializeToString(result);
        var data = JsonSerializer.Deserialize<MazeStatsData>(statsOutput, JsonOptions);

        // Assert
        Assert.That(data!.Path.ShortestPathLength, Is.GreaterThan(0));
        Assert.That(data.Path.GraphNodes, Is.GreaterThan(0));
        Assert.That(data.Path.GraphEdges, Is.GreaterThan(0));
    }

    [Test]
    public void SerializeStats_WithAgent_ContainsAgentData()
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
            GrowingTreeSettings = new GrowingTreeSettings
            {
                NewestWeight = 100,
                OldestWeight = 0,
                RandomWeight = 0
            }
        };
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);

        // Act
        var statsOutput = _services.MazeStatsSerializer.SerializeToString(result);
        var data = JsonSerializer.Deserialize<MazeStatsData>(statsOutput, JsonOptions);

        // Assert
        Assert.That(data!.Agent, Is.Not.Null);
        Assert.That(data.Agent!.StepsTaken, Is.GreaterThan(0));
        Assert.That(data.Agent.EfficiencyPercent, Is.GreaterThan(0));
    }

    [Test]
    public void SerializeStats_WithoutAgent_AgentIsNull()
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

        // Act
        var statsOutput = _services.MazeStatsSerializer.SerializeToString(result);
        var data = JsonSerializer.Deserialize<MazeStatsData>(statsOutput, JsonOptions);

        // Assert
        Assert.That(data!.Agent, Is.Null);
    }

    [Test]
    public void SerializeStats_ToStream_WritesValidJson()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 3, Y = 3, Z = 1 },
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
        using var stream = new MemoryStream();

        // Act
        _services.MazeStatsSerializer.Serialize(result, stream);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var statsOutput = reader.ReadToEnd();
        var data = JsonSerializer.Deserialize<MazeStatsData>(statsOutput, JsonOptions);

        // Assert
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Dimensions.Width, Is.EqualTo(3));
    }

    [Test]
    public void SerializeStats_ContainsGeneratedTimestamp()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 3, Y = 3, Z = 1 },
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

        // Act
        var statsOutput = _services.MazeStatsSerializer.SerializeToString(result);
        var data = JsonSerializer.Deserialize<MazeStatsData>(statsOutput, JsonOptions);

        // Assert
        Assert.That(data!.GeneratedAt, Is.Not.Null.And.Not.Empty);
        Assert.That(data.GeneratedAt, Does.Match(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}"));
    }

    [Test]
    public void SerializeStats_ContainsEndpointCoordinates()
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
        var model = result.MazeJumper.GetModel();

        // Act
        var statsOutput = _services.MazeStatsSerializer.SerializeToString(result);
        var data = JsonSerializer.Deserialize<MazeStatsData>(statsOutput, JsonOptions);

        // Assert
        Assert.That(data!.Endpoints.Start.X, Is.EqualTo(model.StartPoint.X));
        Assert.That(data.Endpoints.Start.Y, Is.EqualTo(model.StartPoint.Y));
        Assert.That(data.Endpoints.Start.Z, Is.EqualTo(model.StartPoint.Z));
        Assert.That(data.Endpoints.End.X, Is.EqualTo(model.EndPoint.X));
        Assert.That(data.Endpoints.End.Y, Is.EqualTo(model.EndPoint.Y));
        Assert.That(data.Endpoints.End.Z, Is.EqualTo(model.EndPoint.Z));
    }

    [Test]
    public void BuildStatsData_ReturnsPopulatedObject()
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

        // Act
        var statsSerializer = (MazeStatsSerializer)_services.MazeStatsSerializer;
        var data = statsSerializer.BuildStatsData(result);

        // Assert
        Assert.That(data, Is.Not.Null);
        Assert.That(data.Dimensions.Width, Is.EqualTo(5));
        Assert.That(data.Dimensions.Height, Is.EqualTo(5));
        Assert.That(data.Dimensions.TotalCells, Is.EqualTo(25));
        Assert.That(data.Timing.TotalTimeMs, Is.GreaterThan(0));
    }

    [Test]
    public void SerializeStats_DirectionUsage_ContainsDirections()
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

        // Act
        var statsOutput = _services.MazeStatsSerializer.SerializeToString(result);
        var data = JsonSerializer.Deserialize<MazeStatsData>(statsOutput, JsonOptions);

        // Assert
        Assert.That(data!.DirectionUsage.Directions, Is.Not.Empty);
        Assert.That(data.DirectionUsage.MaximumUse, Is.Not.Null);
        Assert.That(data.DirectionUsage.MinimumUse, Is.Not.Null);
    }

    #endregion
}
