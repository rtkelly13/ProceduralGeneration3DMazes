using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public class CellInformationLoader : ICellInformationLoader
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public CellInformationLoader(IDirectionsFlagParser directionsFlagParser)
        {
            _directionsFlagParser = directionsFlagParser;
        }

        public CellSpriteNames LoadCellSpriteNames(Direction flag)
        {
            var spriteName = "";
            if (flag == Direction.None)
            {
                spriteName = "Cover";
            }
            else
            {
                if (_directionsFlagParser.FlagHasDirections(flag,Direction.Left))
                {
                    spriteName += "Left";
                }
                if (_directionsFlagParser.FlagHasDirections(flag, Direction.Right))
                {
                    spriteName += "Right";
                }
                if (_directionsFlagParser.FlagHasDirections(flag, Direction.Forward))
                {
                    spriteName += "Forward";
                }
                if (_directionsFlagParser.FlagHasDirections(flag, Direction.Back))
                {
                    spriteName += "Back";
                }
            }
            if (spriteName == "")
            {
                spriteName = "Uncover";
            }

            return new CellSpriteNames
            {
                Background = spriteName,
                Up = _directionsFlagParser.FlagHasDirections(flag, Direction.Up) ? "UpStairs" : null,
                Down = _directionsFlagParser.FlagHasDirections(flag, Direction.Down) ? "DownStairs" : null
            };
        }
    }
}
