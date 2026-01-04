namespace ProceduralMaze.Maze.Model
{
    public interface IModelBuilder : IModel
    {
        void PlaceVertex(MazePoint p, Direction d);
        void RemoveVertex(MazePoint p, Direction d);
    }
}
