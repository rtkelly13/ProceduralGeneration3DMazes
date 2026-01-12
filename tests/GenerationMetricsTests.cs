using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Factory;
using System.Collections.Generic;

namespace ProceduralMaze.Tests;

[TestFixture]
public class GenerationMetricsTests
{
    private ServiceContainer _services = null!;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceContainer();
    }

    [Test]
    public void AlgorithmRunResults_ContainsMetrics()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 5, Y = 5, Z = 1 },
            Option = MazeType.ArrayBidirectional,
            GrowingTreeSettings = new GrowingTreeSettings { NewestWeight = 100 }
        };

        // Act
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);

        // Assert
        var property = result.GetType().GetProperty("Metrics");
        Assert.That(property, Is.Not.Null, "MazeGenerationResults should have a Metrics property");
        
        var metrics = property?.GetValue(result);
        Assert.That(metrics, Is.Not.Null, "Metrics should be populated after generation");
    }

    [Test]
    public void AlgorithmRunResults_ContainsHeatmap()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            Size = new MazeSize { X = 5, Y = 5, Z = 1 },
            Option = MazeType.ArrayBidirectional,
            GrowingTreeSettings = new GrowingTreeSettings { NewestWeight = 100 }
        };

        // Act
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);

        // Assert
        var property = result.GetType().GetProperty("Heatmap");
        Assert.That(property, Is.Not.Null, "MazeGenerationResults should have a Heatmap property");
        
        var heatmap = property?.GetValue(result) as Dictionary<MazePoint, int>;
        Assert.That(heatmap, Is.Not.Null, "Heatmap should be populated");
        Assert.That(heatmap!.Count, Is.GreaterThan(0), "Heatmap should have data");
    }
}