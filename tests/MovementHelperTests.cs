using Moq;
using NUnit.Framework;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Tests;

/// <summary>
/// Tests for the MovementHelper class.
/// Migrated from the original ProcGenMaze.Test project.
/// </summary>
[TestFixture]
[NonParallelizable] // Tests in this fixture share mock state and cannot run in parallel
public class MovementHelperTests
{
    private IMovementHelper _movementHelper = null!;
    private Mock<IDirectionsFlagParser> _flagParser = null!;
    private Mock<IMazePointFactory> _pointFactory = null!;
    private IPointValidity _pointValidity = null!;

    [SetUp]
    public void Setup()
    {
        _flagParser = new Mock<IDirectionsFlagParser>();
        _pointFactory = new Mock<IMazePointFactory>();
        _pointValidity = new PointValidity();
        _movementHelper = new MovementHelper(_flagParser.Object, _pointFactory.Object, _pointValidity);
    }

    [Test]
    public void OneCell_GetAdjacentPoints_NoDirectionsReturned()
    {
        // Arrange
        var point = new MazePoint(0, 0, 0);
        var size = new MazeSize { Z = 1, Y = 1, X = 1 };

        // Act
        var directions = _movementHelper.AdjacentPoints(point, size).ToList();

        // Assert
        Assert.That(directions.Count, Is.EqualTo(0));
    }

    [Test]
    public void OneCell_GetAdjacentPoints_NoneDirectionReturned()
    {
        // Arrange
        var point = new MazePoint(0, 0, 0);
        var size = new MazeSize { Z = 1, Y = 1, X = 1 };

        // Act
        var direction = _movementHelper.AdjacentPointsFlag(point, size);

        // Assert
        Assert.That(direction, Is.EqualTo(Direction.None));
    }

    [Test]
    public void TwoCells_GetAdjacentPointsToLeftCell_OneDirectionsReturned()
    {
        // Arrange
        var point = new MazePoint(0, 0, 0);
        var size = new MazeSize { Z = 1, Y = 1, X = 2 };

        // Act
        var directions = _movementHelper.AdjacentPoints(point, size).ToList();

        // Assert
        Assert.That(directions.Count, Is.EqualTo(1));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Right));
    }

    [Test]
    public void TwoCells_GetAdjacentPointsToRightCell_OneDirectionsReturned()
    {
        // Arrange
        var point = new MazePoint(1, 0, 0);
        var size = new MazeSize { Z = 1, Y = 1, X = 2 };

        // Act
        var directions = _movementHelper.AdjacentPoints(point, size).ToList();

        // Assert
        Assert.That(directions.Count, Is.EqualTo(1));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Left));
    }

    [Test]
    public void EightCells_GetAdjacentPointsToRightUpForwardCell_ThreeDirectionsReturned()
    {
        // Arrange
        var point = new MazePoint(1, 1, 1);
        var size = new MazeSize { Z = 2, Y = 2, X = 2 };

        // Act
        var directions = _movementHelper.AdjacentPoints(point, size).ToList();

        // Assert
        Assert.That(directions.Count, Is.EqualTo(3));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Left));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Down));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Back));
    }

    [Test]
    public void EightCells_GetAdjacentPointsToLeftUpForwardCell_ThreeDirectionsReturned()
    {
        // Arrange
        var point = new MazePoint(0, 1, 1);
        var size = new MazeSize { Z = 2, Y = 2, X = 2 };

        // Act
        var directions = _movementHelper.AdjacentPoints(point, size).ToList();

        // Assert
        Assert.That(directions.Count, Is.EqualTo(3));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Right));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Down));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Back));
    }

    [Test]
    public void EightCells_GetAdjacentPointsToLeftUpBackCell_ThreeDirectionsReturned()
    {
        // Arrange
        var point = new MazePoint(0, 0, 1);
        var size = new MazeSize { Z = 2, Y = 2, X = 2 };

        // Act
        var directions = _movementHelper.AdjacentPoints(point, size).ToList();

        // Assert
        Assert.That(directions.Count, Is.EqualTo(3));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Right));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Down));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Forward));
    }

    [Test]
    public void EightCells_GetAdjacentPointsToLeftDownBackCell_ThreeDirectionsReturned()
    {
        // Arrange
        var point = new MazePoint(0, 0, 0);
        var size = new MazeSize { Z = 2, Y = 2, X = 2 };

        // Act
        var directions = _movementHelper.AdjacentPoints(point, size).ToList();

        // Assert
        Assert.That(directions.Count, Is.EqualTo(3));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Right));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Up));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Forward));
    }

    [Test]
    public void TwentySevenCells_GetAdjacentPointsToCentreCell_SixDirectionsReturned()
    {
        // Arrange
        var point = new MazePoint(1, 1, 1);
        var size = new MazeSize { Z = 3, Y = 3, X = 3 };

        // Act
        var directions = _movementHelper.AdjacentPoints(point, size).ToList();

        // Assert
        Assert.That(directions.Count, Is.EqualTo(6));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Left));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Right));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Forward));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Back));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Up));
        Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Down));
    }

    [Test]
    public void TwentySevenCells_GetAdjacentPointsToCentreCell_AllDirectionReturned()
    {
        // Arrange
        _flagParser.Setup(x => x.AddDirectionsToFlag(It.IsAny<Direction>(), It.IsAny<Direction>())).Returns(
            (Direction seed, Direction d) =>
            {
                var flag = seed | d;
                return flag;
            });

        var point = new MazePoint(1, 1, 1);
        var size = new MazeSize { Z = 3, Y = 3, X = 3 };

        // Act
        var direction = _movementHelper.AdjacentPointsFlag(point, size);

        // Assert
        Assert.That(direction, Is.EqualTo(Direction.All));
    }
}
