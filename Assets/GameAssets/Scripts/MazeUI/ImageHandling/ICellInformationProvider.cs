namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public interface ICellInformationProvider
    {
        int CellSize { get; }
        int LineHeight { get; }
        int LineWidth { get; }

        int HalfLineWidth { get;  }
        int HalfLineHeight { get; }
        int HalfCellSize { get; }

        int QuarterLineWidth { get; }
        int QuarterLineHeight { get; }
        int QuarterCellSize { get; }
    }
}