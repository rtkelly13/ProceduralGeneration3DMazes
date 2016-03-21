using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze
{
    public interface IBuilder
    {
        void PlaceVertex(MazePoint p, Direction d);
        void RemoveVertex(MazePoint p, Direction d);
    }
}
