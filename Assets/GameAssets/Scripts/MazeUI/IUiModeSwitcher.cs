namespace Assets.GameAssets.Scripts.MazeUI
{
    public interface IUiModeSwitcher
    {
        UiMode GetNext(UiMode mode);
    }
}