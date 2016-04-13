using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class UiModeSwitcher : IUiModeSwitcher
    {
        public UiModeSwitcher()
        {
            Mode = UiMode.ShortestPath;
        }

        public UiMode Mode { get; set; }

        public UiMode GetNext()
        {
            switch (Mode)
            {
                case UiMode.ShortestPath:
                    Mode = UiMode.DeadEndLess;
                    break;
                case UiMode.DeadEndLess:
                    Mode = UiMode.Agent;
                    break;
                case UiMode.Agent:
                    Mode = UiMode.ShortestPath;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Mode;
        }
    }
}
