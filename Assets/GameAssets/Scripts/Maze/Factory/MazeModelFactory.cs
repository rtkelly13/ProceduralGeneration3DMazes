using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine.Networking;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public class MazeModelFactory : IMazeModelFactory
    {
        private readonly IMovementHelper _movementHelper;
        private readonly IDirectionsFlagParser _parser;
        private readonly IMazePointFactory _pointFactory;

        public MazeModelFactory(IMovementHelper movementHelper, IDirectionsFlagParser parser, IMazePointFactory pointFactory)
        {
            _movementHelper = movementHelper;
            _parser = parser;
            _pointFactory = pointFactory;
        }

        public MazeBase BuildMaze(MazeType type, MazeSize size)
        {
            switch (type)
            {
                case MazeType.None:
                    throw new ArgumentException("Maze Type None is not supported");
                case MazeType.UndirectedMaze:
                    return new MazeModel2 (_parser, _pointFactory, _movementHelper).BaseInitialise(size);
                case MazeType.DirectedMaze:
                    return new MazeModel1(_parser, _pointFactory, _movementHelper).BaseInitialise(size);
                case MazeType.DictionaryMaze:
                    return new MazeModel3(_parser, _pointFactory, _movementHelper).BaseInitialise(size);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
