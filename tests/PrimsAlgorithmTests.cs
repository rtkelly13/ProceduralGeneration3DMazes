using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Generation;
using System.Linq;

namespace ProceduralMaze.Tests;

[TestFixture]
public class PrimsAlgorithmTests
{
    private ServiceContainer _services = null!;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceContainer();
    }

    [Test]
    public void PrimsAlgorithm_CreatesValidMaze_2D()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.None, // We'll manually call the algorithm for now
            Size = new MazeSize { X = 10, Y = 10, Z = 1 },
            Option = MazeType.ArrayBidirectional
        };
        var model = _services.MazeModelFactory.BuildMaze(settings);
        var carver = _services.MazeFactory.GetMazeCarver(model);
        var prims = new PrimsAlgorithm(_services.DirectionsFlagParser, _services.RandomPointGenerator, _services.RandomValueGenerator);

        // Act
        var result = prims.GenerateMaze(carver, settings);
        var mazeJumper = result.Carver.CarvingFinished();

        // Assert
        Assert.That(result.DirectionsCarvedIn.Count, Is.EqualTo(99)); // 100 cells, 99 connections for perfect maze
        
        // Check connectivity by ensuring all cells have at least one direction
        int cellsWithDirections = 0;
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                mazeJumper.JumpToPoint(new MazePoint(x, y, 0));
                if (mazeJumper.GetFlagFromPoint() != Direction.None) cellsWithDirections++;
            }
        }
        Assert.That(cellsWithDirections, Is.EqualTo(100));
    }

    [Test]
    public void PrimsAlgorithm_CreatesValidMaze_3D()
    {
        // Arrange
        var settings = new MazeGenerationSettings
        {
            Size = new MazeSize { X = 5, Y = 5, Z = 3 },
            Option = MazeType.ArrayBidirectional
        };
        var model = _services.MazeModelFactory.BuildMaze(settings);
        var carver = _services.MazeFactory.GetMazeCarver(model);
        var prims = new PrimsAlgorithm(_services.DirectionsFlagParser, _services.RandomPointGenerator, _services.RandomValueGenerator);

        // Act
        var result = prims.GenerateMaze(carver, settings);
        var mazeJumper = result.Carver.CarvingFinished();

        // Assert
        Assert.That(result.DirectionsCarvedIn.Count, Is.EqualTo(5 * 5 * 3 - 1));

        // Check for vertical connections
        bool hasVertical = false;
        for (int z = 0; z < 3; z++)
        {
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    mazeJumper.JumpToPoint(new MazePoint(x, y, z));
                    var directions = mazeJumper.GetFlagFromPoint();
                    if ((directions & (Direction.Up | Direction.Down)) != 0)
                    {
                        hasVertical = true;
                        break;
                    }
                }
                if (hasVertical) break;
            }
            if (hasVertical) break;
        }
        Assert.That(hasVertical, Is.True, "3D maze should have vertical connections");
    }
}
