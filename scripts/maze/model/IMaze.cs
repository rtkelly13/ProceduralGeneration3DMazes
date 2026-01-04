using System;

namespace ProceduralMaze.Maze.Model
{
    public interface IMaze : IModelState
    {
        Direction GetFlagFromPoint();
        Direction[] GetDirectionsFromPoint();

        bool HasVertexes(Direction flag);
        void MoveInDirection(Direction d);
        bool CanMoveInDirection(Direction d);

        MazePoint CurrentPoint { get; }
        MazeSize Size { get; }

        MazePoint StartPoint { get; }
        MazePoint EndPoint { get; }

        void DoDeadEndWrapping(Func<IModelBuilder, IDeadEndModelWrapper> modelAction);
    }
}
