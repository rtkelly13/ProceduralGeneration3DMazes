using System;
using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public interface IMaze
    {
        Direction GetFlagFromPoint();
        IEnumerable<Direction> GetsDirectionsFromPoint();

        bool HasVertexes(Direction flag);
        void MoveInDirection(Direction d);
        bool CanMoveInDirection(Direction d);

        MazePoint CurrentPoint { get; }
        MazeSize Size { get; }

        MazePoint StartPoint { get; }
        MazePoint EndPoint { get; }

        bool DeadEnded { get; }
        void ToggleDeadEnd();
        void DoDeadEndWrapping(Func<IModelBuilder, IDeadEndModelWrapper> modelAction);
    }
}
