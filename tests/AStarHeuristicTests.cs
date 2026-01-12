using NUnit.Framework;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver.Heuristics;

namespace ProceduralMaze.Tests;

[TestFixture]
public class AStarHeuristicTests
{
    [Test]
    public void ManhattanHeuristic_CalculatesCorrectly_2D()
    {
        // Arrange
        var p1 = new MazePoint(0, 0, 0);
        var p2 = new MazePoint(3, 4, 0);
        var heuristic = new ManhattanHeuristic();

        // Act
        var distance = heuristic.Calculate(p1, p2);

        // Assert
        Assert.That(distance, Is.EqualTo(7.0)); // 3 + 4 + 0
    }

    [Test]
    public void ManhattanHeuristic_CalculatesCorrectly_3D()
    {
        // Arrange
        var p1 = new MazePoint(1, 1, 1);
        var p2 = new MazePoint(3, 4, 5);
        var heuristic = new ManhattanHeuristic();

        // Act
        var distance = heuristic.Calculate(p1, p2);

        // Assert
        Assert.That(distance, Is.EqualTo(9.0)); // |1-3| + |1-4| + |1-5| = 2 + 3 + 4 = 9
    }

    [Test]
    public void EuclideanHeuristic_CalculatesCorrectly_2D()
    {
        // Arrange
        var p1 = new MazePoint(0, 0, 0);
        var p2 = new MazePoint(3, 4, 0);
        var heuristic = new EuclideanHeuristic();

        // Act
        var distance = heuristic.Calculate(p1, p2);

        // Assert
        Assert.That(distance, Is.EqualTo(5.0)); // sqrt(3^2 + 4^2) = 5
    }

    [Test]
    public void EuclideanHeuristic_CalculatesCorrectly_3D()
    {
        // Arrange
        var p1 = new MazePoint(0, 0, 0);
        var p2 = new MazePoint(1, 2, 2);
        var heuristic = new EuclideanHeuristic();

        // Act
        var distance = heuristic.Calculate(p1, p2);

        // Assert
        Assert.That(distance, Is.EqualTo(3.0)); // sqrt(1^2 + 2^2 + 2^2) = sqrt(9) = 3
    }
}
