using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GameAssets.Scripts.Maze;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;
using Moq;
using NUnit.Framework;

namespace ProcGenMaze.Test
{
    [TestFixture]
    public class MovementHelperTests
    {
        private IMovementHelper _movementHelper;
        private Mock<IDirectionsFlagParser> _flagParser;
        private Mock<IMazePointFactory> _pointFactory;
        private IPointValidity _pointValidity;

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
            //Arrange
            var point = new MazePoint(0, 0, 0);
            var size = new MazeSize { Depth = 1, Height = 1, Width = 1};
            //Act
            var directions = _movementHelper.AdjacentPoints(point, size).ToList();
            //Assert
            Assert.That(directions.Count, Is.EqualTo(0));

        }

        [Test]
        public void OneCell_GetAdjacentPoints_NoneDirectionReturned()
        {
            //Arrange
            var point = new MazePoint(0, 0, 0);
            var size = new MazeSize { Depth = 1, Height = 1, Width = 1 };
            //Act
            var direction = _movementHelper.AdjacentPointsFlag(point, size);
            //Assert
            Assert.That(direction, Is.EqualTo(Direction.None));

        }

        [Test]
        public void NineCells_GetAdjacentPointsToCentreCell_SixDirectionsReturned()
        {
            //Arrange
            var point = new MazePoint(1, 1, 1);
            var size = new MazeSize { Depth = 3, Height = 3, Width = 3 };
            //Act
            var directions = _movementHelper.AdjacentPoints(point, size).ToList();
            //Assert
            Assert.That(directions.Count, Is.EqualTo(6));

            Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Left));
            Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Right));
            Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Forward));
            Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Back));
            Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Up));
            Assert.That(directions, Has.Exactly(1).EqualTo(Direction.Down));

        }

        [Test]
        public void NineCells_GetAdjacentPointsToCentreCell_AllDirectionReturned()
        {
            //Arrange
            _flagParser.Setup(x => x.AddDirectionsToFlag(It.IsAny<Direction>(), It.IsAny<Direction>())).Returns(
                (Direction seed, Direction d) => seed | d);

            var point = new MazePoint(1, 1, 1);
            var size = new MazeSize { Depth = 3, Height = 3, Width = 3 };
            //Act
            var direction = _movementHelper.AdjacentPointsFlag(point, size);
            //Assert
            Assert.That(direction, Is.EqualTo(Direction.All));
        }

    }
}
