namespace Assets.GameAssets.Scripts.MazeUI
{
    public interface IUiModeSwitcher
    {
        UiMode Mode { get; set; }
        UiMode GetNext();
    }
}