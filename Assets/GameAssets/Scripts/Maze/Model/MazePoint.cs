namespace Assets.GameAssets.Scripts.Maze.Model
{
    //Maze Points are Zero Based relative to the size of the maze
    public class MazePoint
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

        public MazePoint(int x, int y, int z)
        {
            Set(x, y, z);
        }

        public MazePoint()
        {
            
        }

        public MazePoint Set(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
            return this;
        }

        protected bool Equals(MazePoint other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MazePoint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }
    }
}
