using System;
using System.Collections.Generic;

namespace ProceduralMaze.Maze.Serialization
{
    /// <summary>
    /// Data class representing maze generation statistics for JSON serialization.
    /// </summary>
    public class MazeStatsData
    {
        public string GeneratedAt { get; set; } = string.Empty;

        public DimensionsData Dimensions { get; set; } = new();
        public EndpointsData Endpoints { get; set; } = new();
        public PathData Path { get; set; } = new();
        public DeadEndsData DeadEnds { get; set; } = new();
        public DirectionUsageData DirectionUsage { get; set; } = new();
        public TimingData Timing { get; set; } = new();
        public AgentData? Agent { get; set; }
    }

    public class DimensionsData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
        public int TotalCells { get; set; }
    }

    public class EndpointsData
    {
        public PointData Start { get; set; } = new();
        public PointData End { get; set; } = new();
    }

    public class PointData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    public class PathData
    {
        public int ShortestPathLength { get; set; }
        public int GraphNodes { get; set; }
        public int GraphEdges { get; set; }
    }

    public class DeadEndsData
    {
        public int CellsFilledIn { get; set; }
    }

    public class DirectionUsageData
    {
        public Dictionary<string, int> Directions { get; set; } = new();
        public DirectionStatData? MaximumUse { get; set; }
        public DirectionStatData? MinimumUse { get; set; }
    }

    public class DirectionStatData
    {
        public string Direction { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TimingData
    {
        public string ModelTime { get; set; } = string.Empty;
        public string GenerationTime { get; set; } = string.Empty;
        public string DeadEndFillerTime { get; set; } = string.Empty;
        public string HeuristicsTime { get; set; } = string.Empty;
        public string? AgentGenerationTime { get; set; }
        public string TotalTime { get; set; } = string.Empty;

        public double ModelTimeMs { get; set; }
        public double GenerationTimeMs { get; set; }
        public double DeadEndFillerTimeMs { get; set; }
        public double HeuristicsTimeMs { get; set; }
        public double? AgentGenerationTimeMs { get; set; }
        public double TotalTimeMs { get; set; }
    }

    public class AgentData
    {
        public int StepsTaken { get; set; }
        public int ShortestPathLength { get; set; }
        public double EfficiencyPercent { get; set; }
    }
}
