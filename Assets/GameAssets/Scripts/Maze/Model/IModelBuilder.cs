namespace Assets.GameAssets.Scripts.Maze.Model
{
    public interface IModelBuilder: IModel
    {
        void PlaceVertex(MazePoint p, Direction d);
        void RemoveVertex(MazePoint p, Direction d);
    }
}
