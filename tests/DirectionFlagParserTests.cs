using NUnit.Framework;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Tests;

/// <summary>
/// Tests for the DirectionsFlagParser class.
/// Migrated from the original ProcGenMaze.Test project.
/// </summary>
[TestFixture]
public class DirectionFlagParserTests
{
    private IDirectionsFlagParser _flagParser = null!;

    [SetUp]
    public void Setup()
    {
        _flagParser = new DirectionsFlagParser();
    }

    [Test]
    public void AllFlagSplit_CorrectFlagsReturned()
    {
        // Act
        var allFlags = _flagParser.SplitDirectionsFromFlag(Direction.All).ToList();

        // Assert
        Assert.That(allFlags.Count, Is.EqualTo(6));

        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Left));
        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Right));
        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Forward));
        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Back));
        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Up));
        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Down));
    }

    [Test]
    public void XAxisFlagSplit_CorrectFlagsReturned()
    {
        // Act
        var allFlags = _flagParser.SplitDirectionsFromFlag(Direction.XAxis).ToList();

        // Assert
        Assert.That(allFlags.Count, Is.EqualTo(2));

        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Left));
        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Right));
    }

    [Test]
    public void YAxisFlagSplit_CorrectFlagsReturned()
    {
        // Act
        var allFlags = _flagParser.SplitDirectionsFromFlag(Direction.YAxis).ToList();

        // Assert
        Assert.That(allFlags.Count, Is.EqualTo(2));

        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Forward));
        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Back));
    }

    [Test]
    public void ZAxisFlagSplit_CorrectFlagsReturned()
    {
        // Act
        var allFlags = _flagParser.SplitDirectionsFromFlag(Direction.ZAxis).ToList();

        // Assert
        Assert.That(allFlags.Count, Is.EqualTo(2));

        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Up));
        Assert.That(allFlags, Has.Exactly(1).EqualTo(Direction.Down));
    }

    [Test]
    public void RightFlag_AddLeft_BothFlagsRemain()
    {
        // Arrange
        var right = Direction.Right;

        // Act
        var final = _flagParser.AddDirectionsToFlag(right, Direction.Left);
        
        // Assert
        Assert.That(final, Is.EqualTo(Direction.Left | Direction.Right));
    }

    [Test]
    public void RightFlag_AddRight_RightFlagRemains()
    {
        // Arrange
        var right = Direction.Right;
        
        // Act
        var final = _flagParser.AddDirectionsToFlag(right, Direction.Right);

        // Assert
        Assert.That(final, Is.EqualTo(Direction.Right));
    }

    [Test]
    public void LeftFlag_OppositeDirection_RightFlagReturned()
    {
        // Arrange
        var left = Direction.Left;

        // Act
        var final = _flagParser.OppositeDirection(left);

        // Assert
        Assert.That(final, Is.EqualTo(Direction.Right));
    }

    [Test]
    public void RightFlag_OppositeDirection_LeftFlagReturned()
    {
        // Arrange
        var right = Direction.Right;

        // Act
        var final = _flagParser.OppositeDirection(right);

        // Assert
        Assert.That(final, Is.EqualTo(Direction.Left));
    }

    [Test]
    public void UpFlag_OppositeDirection_DownFlagReturned()
    {
        // Arrange
        var up = Direction.Up;

        // Act
        var final = _flagParser.OppositeDirection(up);

        // Assert
        Assert.That(final, Is.EqualTo(Direction.Down));
    }

    [Test]
    public void DownFlag_OppositeDirection_UpFlagReturned()
    {
        // Arrange
        var down = Direction.Down;

        // Act
        var final = _flagParser.OppositeDirection(down);

        // Assert
        Assert.That(final, Is.EqualTo(Direction.Up));
    }

    [Test]
    public void ForwardFlag_OppositeDirection_BackFlagReturned()
    {
        // Arrange
        var forward = Direction.Forward;

        // Act
        var final = _flagParser.OppositeDirection(forward);

        // Assert
        Assert.That(final, Is.EqualTo(Direction.Back));
    }

    [Test]
    public void BackFlag_OppositeDirection_ForwardFlagReturned()
    {
        // Arrange
        var back = Direction.Back;

        // Act
        var final = _flagParser.OppositeDirection(back);

        // Assert
        Assert.That(final, Is.EqualTo(Direction.Forward));
    }

    [Test]
    public void AllFlag_OppositeDirection_ExceptionThrown()
    {
        // Arrange
        var all = Direction.All;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            _flagParser.OppositeDirection(all);
        });
    }

    [Test]
    public void AllFlag_HasLeftDirection_True()
    {
        // Arrange
        var all = Direction.All;

        // Act
        var result = _flagParser.FlagHasDirections(all, Direction.Left);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void AllFlag_HasXAxisFlag_True()
    {
        // Arrange
        var all = Direction.All;

        // Act
        var result = _flagParser.FlagHasDirections(all, Direction.XAxis);
    
        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void LeftFlag_DoesNotHaveUpDirection_False()
    {
        // Arrange
        var left = Direction.Left;

        // Act
        var result = _flagParser.FlagHasDirections(left, Direction.Up);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void LeftDirection_IsDirection_True()
    {
        // Arrange
        var left = Direction.Left;

        // Act
        var result = _flagParser.IsDirection(left);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void AllFlag_IsDirection_False()
    {
        // Arrange
        var all = Direction.All;

        // Act
        var result = _flagParser.IsDirection(all);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void XAxis_RemoveLeft_RightFlag()
    {
        // Arrange
        var xAxis = Direction.XAxis;

        // Act
        var result = _flagParser.RemoveDirectionsFromFlag(xAxis, Direction.Left);

        // Assert
        Assert.That(result, Is.EqualTo(Direction.Right));
    }

    [Test]
    public void AllFlag_RemoveLeft_RightYAxisAndZAxis()
    {
        // Arrange
        var all = Direction.All;

        // Act
        var result = _flagParser.RemoveDirectionsFromFlag(all, Direction.Left);

        // Assert
        Assert.That(result, Is.EqualTo(Direction.Right | Direction.YAxis | Direction.ZAxis));
    }

    [Test]
    public void AllFlag_OppositeFlag_NoneFlag()
    {
        // Arrange
        var all = Direction.All;

        // Act
        var result = _flagParser.OppositeFlag(all);

        // Assert
        Assert.That(result, Is.EqualTo(Direction.None));
    }

    [Test]
    public void XAxis_OppositeFlag_YAxisAndZAxis()
    {
        // Arrange
        var xAxis = Direction.XAxis;

        // Act
        var result = _flagParser.OppositeFlag(xAxis);

        // Assert
        Assert.That(result, Is.EqualTo(Direction.YAxis | Direction.ZAxis));
    }

    [Test]
    public void Left_OppositeFlag_RightYAxisAndZAxis()
    {
        // Arrange
        var left = Direction.Left;

        // Act
        var result = _flagParser.OppositeFlag(left);

        // Assert
        Assert.That(result, Is.EqualTo(Direction.Right | Direction.YAxis | Direction.ZAxis));
    }

    [Test]
    public void XAxisAndYAxis_RemoveLeftDirectionsFromFlag_RightYAxisRemaining()
    {
        // Arrange
        var flag = Direction.XAxis | Direction.YAxis;

        // Act
        var result = _flagParser.RemoveDirectionsFromFlag(flag, Direction.Left);

        // Assert
        Assert.That(result, Is.EqualTo(Direction.Right | Direction.YAxis));
    }

    [Test]
    public void XAxisAndYAxis_RemoveXAxisDirectionsFromFlag_YAxisRemaining()
    {
        // Arrange
        var flag = Direction.XAxis | Direction.YAxis;

        // Act
        var result = _flagParser.RemoveDirectionsFromFlag(flag, Direction.XAxis);

        // Assert
        Assert.That(result, Is.EqualTo(Direction.YAxis));
    }
}
