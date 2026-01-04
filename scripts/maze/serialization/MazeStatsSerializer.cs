using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProceduralMaze.Maze.Factory;

namespace ProceduralMaze.Maze.Serialization
{
    /// <summary>
    /// Serializes maze generation statistics to JSON format.
    /// </summary>
    public class MazeStatsSerializer : IMazeStatsSerializer
    {
        public const string StatsFileExtension = ".stats.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public void Serialize(MazeGenerationResults results, Stream output)
        {
            var data = BuildStatsData(results);
            JsonSerializer.Serialize(output, data, JsonOptions);
        }

        public string SerializeToString(MazeGenerationResults results)
        {
            var data = BuildStatsData(results);
            return JsonSerializer.Serialize(data, JsonOptions);
        }

        /// <summary>
        /// Builds a MazeStatsData object from maze generation results.
        /// </summary>
        public MazeStatsData BuildStatsData(MazeGenerationResults results)
        {
            var model = results.MazeJumper.GetModel();
            var heuristics = results.HeuristicsResults;
            var deadEndFiller = results.DeadEndFillerResults;
            var stats = heuristics.Stats;

            // Calculate total edges
            var totalEdges = 0;
            foreach (var node in heuristics.ShortestPathResult.Graph.Nodes.Values)
            {
                totalEdges += node.Edges.Length;
            }

            var data = new MazeStatsData
            {
                GeneratedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                Dimensions = new DimensionsData
                {
                    Width = model.Size.X,
                    Height = model.Size.Y,
                    Depth = model.Size.Z,
                    TotalCells = heuristics.TotalCells
                },
                Endpoints = new EndpointsData
                {
                    Start = new PointData
                    {
                        X = model.StartPoint.X,
                        Y = model.StartPoint.Y,
                        Z = model.StartPoint.Z
                    },
                    End = new PointData
                    {
                        X = model.EndPoint.X,
                        Y = model.EndPoint.Y,
                        Z = model.EndPoint.Z
                    }
                },
                Path = new PathData
                {
                    ShortestPathLength = heuristics.ShortestPathResult.ShortestPath,
                    GraphNodes = heuristics.ShortestPathResult.Graph.Nodes.Count,
                    GraphEdges = totalEdges
                },
                DeadEnds = new DeadEndsData
                {
                    CellsFilledIn = deadEndFiller.TotalCellsFilledIn
                },
                DirectionUsage = new DirectionUsageData()
            };

            // Direction usage
            foreach (var kvp in stats.DirectionsUsed)
            {
                data.DirectionUsage.Directions[kvp.Key.ToString()] = kvp.Value;
            }

            if (stats.MaximumUse != null)
            {
                data.DirectionUsage.MaximumUse = new DirectionStatData
                {
                    Direction = stats.MaximumUse.Direction.ToString(),
                    Count = stats.MaximumUse.NumberOfUsages
                };
            }

            if (stats.MinimumUse != null)
            {
                data.DirectionUsage.MinimumUse = new DirectionStatData
                {
                    Direction = stats.MinimumUse.Direction.ToString(),
                    Count = stats.MinimumUse.NumberOfUsages
                };
            }

            // Timing
            data.Timing = new TimingData
            {
                ModelTime = FormatTimeSpan(results.ModelTime),
                GenerationTime = FormatTimeSpan(results.GenerationTime),
                DeadEndFillerTime = FormatTimeSpan(results.DeadEndFillerTime),
                HeuristicsTime = FormatTimeSpan(results.HeuristicsTime),
                TotalTime = FormatTimeSpan(results.TotalTime),
                ModelTimeMs = Math.Round(results.ModelTime.TotalMilliseconds, 3),
                GenerationTimeMs = Math.Round(results.GenerationTime.TotalMilliseconds, 3),
                DeadEndFillerTimeMs = Math.Round(results.DeadEndFillerTime.TotalMilliseconds, 3),
                HeuristicsTimeMs = Math.Round(results.HeuristicsTime.TotalMilliseconds, 3),
                TotalTimeMs = Math.Round(results.TotalTime.TotalMilliseconds, 3)
            };

            if (results.AgentResults != null)
            {
                data.Timing.AgentGenerationTime = FormatTimeSpan(results.AgentGenerationTime);
                data.Timing.AgentGenerationTimeMs = Math.Round(results.AgentGenerationTime.TotalMilliseconds, 3);

                var stepsTaken = results.AgentResults.Movements.Count;
                var shortestPath = heuristics.ShortestPathResult.ShortestPath;
                var efficiency = shortestPath > 0 ? (double)shortestPath / stepsTaken * 100 : 0;

                data.Agent = new AgentData
                {
                    StepsTaken = stepsTaken,
                    ShortestPathLength = shortestPath,
                    EfficiencyPercent = Math.Round(efficiency, 2)
                };
            }

            return data;
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalMilliseconds < 1)
            {
                return $"{ts.TotalMicroseconds:F0}us";
            }
            if (ts.TotalSeconds < 1)
            {
                return $"{ts.TotalMilliseconds:F2}ms";
            }
            if (ts.TotalMinutes < 1)
            {
                return $"{ts.TotalSeconds:F2}s";
            }
            return $"{ts.TotalMinutes:F2}min";
        }
    }
}
