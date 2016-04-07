namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public interface IDoorwayLoader
    {
        CirclePrefab GetDoor(DoorType type);
    }
}