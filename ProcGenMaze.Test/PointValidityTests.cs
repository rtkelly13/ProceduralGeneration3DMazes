using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;
using Moq;
using NUnit.Framework;

namespace ProcGenMaze.Test
{
    [TestFixture]
    public class PointValidityTests
    {
        private IPointValidity _pointValidity;
        [SetUp]
        public void Setup()
        {
            _pointValidity = new PointValidity();
        }

        [Test]
        public void OneCell_PassInAValidCell_CellIsValid()
        {
            //Arrange
            var point = new MazePoint(0, 0, 0);
            var size = new MazeSize { Z = 1, Y = 1, X = 1 };
            //Act
            var valid = _pointValidity.ValidPoint(point, size);
            //Assert
            Assert.That(valid, Is.True);
        }

        [Test]
        public void OneCell_PassInAInvalidCellTooLow_CellIsInvalid()
        {
            //Arrange
            var point = new MazePoint(-1, 0, 0);
            var size = new MazeSize { Z = 1, Y = 1, X = 1 };
            //Act
            var valid = _pointValidity.ValidPoint(point, size);
            //Assert
            Assert.That(valid, Is.False);
        }

        [Test]
        public void OneCell_PassInAInvalidCellTooHigh_CellIsInvalid()
        {
            //Arrange
            var point = new MazePoint(1, 0, 0);
            var size = new MazeSize { Z = 1, Y = 1, X = 1 };
            //Act
            var valid = _pointValidity.ValidPoint(point, size);
            //Assert
            Assert.That(valid, Is.False);
        }
    }
}
