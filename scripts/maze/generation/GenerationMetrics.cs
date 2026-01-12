namespace ProceduralMaze.Maze.Generation
{
    public class GenerationMetrics
    {
        public int Steps { get; set; }
        public int DeadEnds { get; set; }
        public int Junctions { get; set; }
        public double BranchingFactor { get; set; }
    }
}
