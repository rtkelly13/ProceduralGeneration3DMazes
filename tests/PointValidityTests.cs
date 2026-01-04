using NUnit.Framework;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Tests;

/// <summary>
/// Tests for the PointValidity class.
/// Migrated from the original ProcGenMaze.Test project.
/// </summary>
[TestFixture]
public class PointValidityTests
{
    private IPointValidity _pointValidity = null!;

    [SetUp]
    public void Setup()
    {
        _pointValidity = new PointValidity();
    }

    [Test]
    public void OneCell_PassInAValidCell_CellIsValid()
    {
        // Arrange
        var point = new MazePoint(0, 0, 0);
        var size = new MazeSize { Z = 1, Y = 1, X = 1 };

        // Act
        var valid = _pointValidity.ValidPoint(point, size);

        // Assert
        Assert.That(valid, Is.True);
    }

    [Test]
    public void OneCell_PassInAInvalidCellTooLow_CellIsInvalid()
    {
        // Arrange
        var point = new MazePoint(-1, 0, 0);
        var size = new MazeSize { Z = 1, Y = 1, X = 1 };

        // Act
        var valid = _pointValidity.ValidPoint(point, size);

        // Assert
        Assert.That(valid, Is.False);
    }

    [Test]
    public void OneCell_PassInAInvalidCellTooHigh_CellIsInvalid()
    {
        // Arrange
        var point = new MazePoint(1, 0, 0);
        var size = new MazeSize { Z = 1, Y = 1, X = 1 };

        // Act
        var valid = _pointValidity.ValidPoint(point, size);

        // Assert
        Assert.That(valid, Is.False);
    }

    [Test]
    public void LargeMaze_CornerPoints_AreValid()
    {
        // Arrange
        var size = new MazeSize { X = 10, Y = 10, Z = 5 };

        // Act & Assert - all corner points should be valid
        Assert.That(_pointValidity.ValidPoint(new MazePoint(0, 0, 0), size), Is.True);
        Assert.That(_pointValidity.ValidPoint(new MazePoint(9, 0, 0), size), Is.True);
        Assert.That(_pointValidity.ValidPoint(new MazePoint(0, 9, 0), size), Is.True);
        Assert.That(_pointValidity.ValidPoint(new MazePoint(0, 0, 4), size), Is.True);
        Assert.That(_pointValidity.ValidPoint(new MazePoint(9, 9, 4), size), Is.True);
    }

    [Test]
    public void LargeMaze_PointsJustOutside_AreInvalid()
    {
        // Arrange
        var size = new MazeSize { X = 10, Y = 10, Z = 5 };

        // Act & Assert - points just outside bounds should be invalid
        Assert.That(_pointValidity.ValidPoint(new MazePoint(10, 0, 0), size), Is.False);
        Assert.That(_pointValidity.ValidPoint(new MazePoint(0, 10, 0), size), Is.False);
        Assert.That(_pointValidity.ValidPoint(new MazePoint(0, 0, 5), size), Is.False);
        Assert.That(_pointValidity.ValidPoint(new MazePoint(-1, 5, 2), size), Is.False);
        Assert.That(_pointValidity.ValidPoint(new MazePoint(5, -1, 2), size), Is.False);
        Assert.That(_pointValidity.ValidPoint(new MazePoint(5, 5, -1), size), Is.False);
    }
}
