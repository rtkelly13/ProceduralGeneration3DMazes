using Moq;
using NUnit.Framework;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Tests;

/// <summary>
/// Tests for random value and point generation.
/// Migrated from the original ProcGenMaze.Test project.
/// </summary>
[TestFixture]
public class RandomValueTests
{
    private IRandomValueGenerator _random = null!;
    private IRandomPointGenerator _randomPoint = null!;
    private Mock<IMazePointFactory> _mazePointFactory = null!;

    [SetUp]
    public void Setup()
    {
        _random = new RandomValueGenerator();
        _mazePointFactory = new Mock<IMazePointFactory>();
        _mazePointFactory.Setup(x => x.MakePoint(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int x, int y, int z) => new MazePoint(x, y, z));
        _randomPoint = new RandomPointGenerator(_random, _mazePointFactory.Object);
    }

    [Test]
    public void SingleValue_GenerateValueBetween1and10_NumberFoundBetween1and10()
    {
        // Act
        var value = _random.GetNext(1, 10);

        // Assert
        Assert.That(value, Is.InRange(1, 10));
    }

    [Test]
    public void HundredValues_GenerateValuesBetween1and10_NumberFoundBetween1and10()
    {
        // Act
        var values = Enumerable.Range(0, 100).Select(_ => _random.GetNext(1, 10)).ToList();
        
        // Assert
        foreach (var value in values)
        {
            Assert.That(value, Is.InRange(1, 10));
        }
    }

    [Test]
    public void SingleRandomPoint_GenerateRandomPointFromSize_IsWithinBounds()
    {
        // Arrange
        var size = new MazeSize
        {
            X = 10, Z = 11, Y = 12
        };

        // Act
        var point = _randomPoint.RandomPoint(size, PickType.Random);

        // Assert
        Assert.That(point.X, Is.InRange(0, 9));
        Assert.That(point.Z, Is.InRange(0, 10));
        Assert.That(point.Y, Is.InRange(0, 11));
    }

    [Test]
    public void HundredRandomPoints_GenerateRandomPointsFromSize_AllWithinBounds()
    {
        // Arrange
        var size = new MazeSize
        {
            X = 10,
            Z = 11,
            Y = 12
        };

        // Act
        var points = Enumerable.Range(0, 100).Select(_ => _randomPoint.RandomPoint(size, PickType.Random)).ToList();

        // Assert
        foreach (var point in points)
        {
            Assert.That(point.X, Is.InRange(0, 9));
            Assert.That(point.Z, Is.InRange(0, 10));
            Assert.That(point.Y, Is.InRange(0, 11));
        }
    }

    [Test]
    public void RandomEdgePoint_GenerateRandomEdgePointFromSize_IsAtEdge()
    {
        // Arrange - use 3D maze so all 6 edge options are meaningful
        var size = new MazeSize { X = 10, Y = 10, Z = 5 };

        // Act - generate many points and verify they're at edges
        var points = Enumerable.Range(0, 100).Select(_ => _randomPoint.RandomPoint(size, PickType.RandomEdge)).ToList();

        // Assert - each point should be at an edge in at least one dimension
        // (x=0, x=9, y=0, y=9, z=0, or z=4)
        foreach (var point in points)
        {
            bool isAtEdge = point.X == 0 || point.X == 9 || 
                           point.Y == 0 || point.Y == 9 ||
                           point.Z == 0 || point.Z == 4;
            Assert.That(isAtEdge, Is.True, $"Point ({point.X},{point.Y},{point.Z}) should be at an edge");
        }
    }

    [Test]
    public void RandomValueGenerator_GetNextWithSameRange_ProducesVariedResults()
    {
        // Act - generate many values
        var values = Enumerable.Range(0, 100).Select(_ => _random.GetNext(0, 100)).ToList();
        
        // Assert - should have more than just one unique value (verifies randomness)
        var uniqueValues = values.Distinct().Count();
        Assert.That(uniqueValues, Is.GreaterThan(1), "Random generator should produce varied results");
    }

    [Test]
    public void RandomValueGenerator_GetNextWithSingleValueRange_ReturnsThatValue()
    {
        // Act - GetNext(5, 5) should return 5 since range is inclusive [5, 5]
        var value = _random.GetNext(5, 5);

        // Assert - when min equals max, result should always be that value
        Assert.That(value, Is.EqualTo(5));
    }

    [Test]
    public void RandomValueGenerator_GetNextIsInclusive_BothBoundsCanBeReturned()
    {
        // Arrange - use a small range and generate many values
        var values = Enumerable.Range(0, 1000).Select(_ => _random.GetNext(0, 1)).ToList();

        // Assert - both 0 and 1 should appear since range is inclusive [0, 1]
        Assert.That(values, Contains.Item(0), "Lower bound should be reachable");
        Assert.That(values, Contains.Item(1), "Upper bound should be reachable");
        Assert.That(values.All(v => v >= 0 && v <= 1), Is.True, "All values should be in range [0, 1]");
    }
}
