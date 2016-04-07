using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Moq;
using NUnit.Framework;

namespace ProcGenMaze.Test
{
    [TestFixture]
    public class RandomValueTests
    {
        private IRandomValueGenerator _random;
        private IRandomPointGenerator _randomPoint;
        private Mock<IMazePointFactory> _mazePointFactory;

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
            //Act
            var value = _random.GetNext(1, 10);

            //Assert
            Assert.That(value, Is.InRange(1, 10));
        }

        [Test]
        public void HundredValues_GenerateValuesBetween1and10_NumberFoundBetween1and10()
        {
            //Act
            var values = Enumerable.Range(0, 100).Select(x => _random.GetNext(1, 10));
            
            //Assert
            foreach (var value in values)
            {
                Assert.That(value, Is.InRange(1, 10));
            }
            
        }

        [Test]
        public void SingleRandomPoint_GenerateRandomPointFromSize_IsRandomPoint()
        {
            //Arrange
            var size = new MazeSize
            {
                X = 10, Z = 11, Y = 12
            };

            //Act
            var point = _randomPoint.RandomPoint(size, PickType.Random);

            //Assert
            Assert.That(point.X, Is.InRange(1, 10));
            Assert.That(point.Z, Is.InRange(1, 11));
            Assert.That(point.Y, Is.InRange(1, 12));
        }

        [Test]
        public void HundredRandomPoint_GenerateRandomPointsFromSize_IsRandomPoint()
        {
            //Arrange
            var size = new MazeSize
            {
                X = 10,
                Z = 11,
                Y = 12
            };

            //Act
            var points = Enumerable.Range(0, 100).Select(x => _randomPoint.RandomPoint(size, PickType.Random));

            //Assert
            foreach (var point in points)
            {
                Assert.That(point.X, Is.InRange(0, 9));
                Assert.That(point.Z, Is.InRange(0, 10));
                Assert.That(point.Y, Is.InRange(0, 11));
            }
            
        }

    }
}
