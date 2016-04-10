using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public class DoorwayLoader : IDoorwayLoader
    {
        private readonly ICircleLoader _circleLoader;

        public DoorwayLoader(ICircleLoader circleLoader)
        {
            _circleLoader = circleLoader;
        }

        public CirclePrefab GetDoor(DoorType type)
        {
            switch (type)
            {
                case DoorType.Entrance:
                    return _circleLoader.GetPrefab("green", "Start");
                case DoorType.Exit:
                    return _circleLoader.GetPrefab("red", "End");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
