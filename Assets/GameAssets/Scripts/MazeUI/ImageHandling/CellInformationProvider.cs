using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public class CellInformationProvider : ICellInformationProvider
    {
        public CellInformationProvider()
        {
            HalfLineWidth = LineWidth / 2;
            HalfLineHeight = LineHeight / 2;
            HalfCellSize = CellSize / 2;
            QuarterLineWidth = HalfLineWidth / 2;
            QuarterLineHeight = HalfLineHeight / 2;
            QuarterCellSize = HalfCellSize / 2;
        }

        public int CellSize { get { return 36; } }
        public int LineWidth { get { return 25; } }
        public int LineHeight { get { return 25; } }

        public int HalfLineWidth { get; private set; }
        public int HalfLineHeight { get; private set; }
        public int HalfCellSize { get; private set; }

        public int QuarterLineWidth { get; private set; }
        public int QuarterLineHeight { get; private set; }
        public int QuarterCellSize { get; private set; }
    }
}
