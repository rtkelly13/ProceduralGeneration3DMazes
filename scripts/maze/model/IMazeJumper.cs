namespace ProceduralMaze.Maze.Model
{
    public interface IMazeJumper : IMaze
    {
        Direction[] JumpableDirections();
        Direction JumpableFlag();

        bool CanJumpInDirection(Direction d);
        bool TryJumpInDirection(Direction d);
        void JumpInDirection(Direction d);

        bool CanJumpToPoint(MazePoint p);
        bool TryJumpToPoint(MazePoint p);
        void JumpToPoint(MazePoint p);

        IMaze JumpingFinished();
        
        /// <summary>
        /// Gets the underlying model for serialization purposes.
        /// </summary>
        IModel GetModel();
    }
}
