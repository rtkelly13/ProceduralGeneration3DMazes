using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze;
using Assets.GameAssets.Scripts.Maze.Helper;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class LoadResourceForFlag
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly ObjectPool<Dictionary<Direction, bool>> _pool;

        public LoadResourceForFlag(IDirectionsFlagParser directionsFlagParser)
        {
            _directionsFlagParser = directionsFlagParser;
            _pool = new ObjectPool<Dictionary<Direction, bool>>(1);
        }

        public CellState LoadCellPrefab(Direction flag)
        {
            var dictionary = _pool.New();
            foreach (var direction in _directionsFlagParser.Directions)
            {
                dictionary[direction] = _directionsFlagParser.FlagHasDirections(flag, direction);
            }
            var up = dictionary[Direction.Up];
            var down = dictionary[Direction.Down];
            var spriteName = "";
            if (dictionary.Values.All(x => !x))
            {
                spriteName = "Cover";
            }
            else
            {
                if (dictionary[Direction.Left])
                {
                    spriteName += "Left";
                }
                if (dictionary[Direction.Right])
                {
                    spriteName += "Right";
                }
                if (dictionary[Direction.Forward])
                {
                    spriteName += "Forward";
                }
                if (dictionary[Direction.Back])
                {
                    spriteName += "Back";
                }
                if (spriteName == "")
                {
                    spriteName = "Uncover";
                }
            }

            Sprite sprite = Resources.Load<Sprite>(spriteName); 
            _pool.Store(dictionary);
            return new CellState
            {
                Background = sprite,
                Up = up,
                Down = down
            };
        }
    }


    public class CellState
    {
        public Sprite Background { get; set; }
        public bool Up { get; set; }
        public bool Down { get; set; }
    }

}
