using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class UiModeSwitcher : IUiModeSwitcher
    {
        public UiMode GetNext(UiMode mode)
        {
            switch (mode)
            {
                case UiMode.ShortestPath:
                    return UiMode.DeadEndLess;
                case UiMode.DeadEndLess:
                    return UiMode.ShortestPath;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
