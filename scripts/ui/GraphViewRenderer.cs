using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.UI
{
    /// <summary>
    /// Renders an abstract graph view of the maze using grid-aware positioning.
    /// </summary>
    public class GraphViewRenderer
    {
        private readonly Node2D _mazeGrid;
        private readonly Action<float, float, float, float> _fitToScreen;

        public GraphViewRenderer(Node2D mazeGrid, Action<float, float, float, float> fitToScreen)
        {
            _mazeGrid = mazeGrid;
            _fitToScreen = fitToScreen;
        }

        public void Render(GameState gameState, bool centerView)
        {
            var graph = gameState.CurrentMaze?.HeuristicsResults?.ShortestPathResult?.Graph;
            if (graph == null) return;

            var mazeJumper = gameState.CurrentMaze!.MazeJumper;
            var visualSettings = gameState.VisualizationSettings;

            // Calculate layout positions
            var nodePositions = CalculateGridAwareLayout(graph, mazeJumper);
            
            if (nodePositions.Count == 0) return;

            // Calculate bounds for fitting to view
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var pos in nodePositions.Values)
            {
                minX = Math.Min(minX, pos.X);
                maxX = Math.Max(maxX, pos.X);
                minY = Math.Min(minY, pos.Y);
                maxY = Math.Max(maxY, pos.Y);
            }

            float graphWidth = maxX - minX + 100;
            float graphHeight = maxY - minY + 100;

            if (centerView)
            {
                _fitToScreen(graphWidth, graphHeight, minX - 50, minY - 50);
            }

            // Check if we're in animation mode
            if (gameState.IsAnimationMode && gameState.AnimationController?.CurrentStep != null)
            {
                RenderAnimationStep(gameState, graph, mazeJumper, nodePositions);
                return;
            }

            // Normal rendering (paths mode)
            RenderPaths(gameState, graph, mazeJumper, nodePositions, visualSettings);
        }

        private void RenderPaths(GameState gameState, Graph graph, IMazeJumper mazeJumper, 
            Dictionary<MazePoint, Vector2> nodePositions, PathVisualizationSettings visualSettings)
        {
            // Determine which paths to render
            var pathsToRender = GetPathsToRender(gameState);
            
            // Build combined edge info for all paths
            var pathEdgesByEdge = new Dictionary<(MazePoint, MazePoint), List<int>>(); // edge -> list of path indices
            var nodesOnAnyPath = new HashSet<MazePoint>();
            
            foreach (var path in pathsToRender)
            {
                foreach (var edge in path.PathEdges)
                {
                    if (!pathEdgesByEdge.ContainsKey(edge))
                    {
                        pathEdgesByEdge[edge] = new List<int>();
                    }
                    pathEdgesByEdge[edge].Add(path.PathIndex);
                }
                foreach (var node in path.JunctionNodes)
                {
                    nodesOnAnyPath.Add(node);
                }
            }

            // Draw edges first (so nodes appear on top)
            var drawnEdges = new HashSet<(MazePoint, MazePoint)>();
            
            foreach (var kvp in graph.Nodes)
            {
                var fromPoint = kvp.Key;
                var node = kvp.Value;

                if (!nodePositions.ContainsKey(fromPoint)) continue;
                var fromPos = nodePositions[fromPoint];

                foreach (var edge in node.Edges)
                {
                    if (!nodePositions.ContainsKey(edge.Point)) continue;
                    
                    var reverseKey = (edge.Point, fromPoint);
                    if (drawnEdges.Contains(reverseKey)) continue;
                    
                    var toPos = nodePositions[edge.Point];
                    var edgeKey = (fromPoint, edge.Point);
                    drawnEdges.Add(edgeKey);

                    // Check if this edge is on any of the paths being rendered
                    var forwardPaths = pathEdgesByEdge.GetValueOrDefault((fromPoint, edge.Point), new List<int>());
                    var backwardPaths = pathEdgesByEdge.GetValueOrDefault((edge.Point, fromPoint), new List<int>());
                    
                    bool isOnAnyPath = forwardPaths.Count > 0 || backwardPaths.Count > 0;
                    var pathIndices = forwardPaths.Concat(backwardPaths).Distinct().OrderBy(x => x).ToList();
                    
                    // Determine color based on path
                    Color edgeColor;
                    float edgeWidth;
                    
                    if (pathIndices.Count > 0)
                    {
                        // Use the lowest path index for primary color (optimal path takes precedence)
                        int primaryPathIndex = pathIndices.Min();
                        edgeColor = visualSettings.GetPathColor(primaryPathIndex);
                        edgeWidth = primaryPathIndex == 0 ? 3f : 2.5f;
                        
                        // If showing all paths and this edge is on multiple paths, slightly dim
                        if (visualSettings.ShowAllPathsSimultaneously && pathIndices.Count > 1)
                        {
                            edgeColor = new Color(edgeColor.R, edgeColor.G, edgeColor.B, 0.8f);
                        }
                    }
                    else
                    {
                        edgeColor = new Color(0.5f, 0.5f, 0.5f);
                        edgeWidth = 1.5f;
                    }
                    
                    // Draw arrows based on path direction
                    bool showArrowToTarget = forwardPaths.Count > 0;
                    bool showArrowToSource = backwardPaths.Count > 0;
                    
                    DrawEdgeWithColor(fromPos, toPos, edge.DirectionsToPoint.Length, edgeColor, edgeWidth, showArrowToTarget, showArrowToSource);
                }
            }

            // Draw nodes on top
            foreach (var kvp in graph.Nodes)
            {
                var point = kvp.Key;
                var node = kvp.Value;

                if (!nodePositions.ContainsKey(point)) continue;
                var pos = nodePositions[point];

                bool isOnPath = nodesOnAnyPath.Contains(point);
                
                // Find which paths this node is on
                var nodePathIndices = pathsToRender
                    .Where(p => p.JunctionNodes.Contains(point))
                    .Select(p => p.PathIndex)
                    .ToList();
                
                DrawNodeWithPaths(pos, point, node.ShortestPath, mazeJumper.StartPoint, mazeJumper.EndPoint, isOnPath, nodePathIndices, visualSettings, node.Edges.Length);
            }
        }

        /// <summary>
        /// Gets the list of paths to render based on current settings.
        /// </summary>
        private List<PathResult> GetPathsToRender(GameState gameState)
        {
            var paths = new List<PathResult>();
            
            if (gameState.AllPaths.Count == 0)
            {
                // Fall back to creating a path result from the shortest path
                var shortestPathResult = gameState.CurrentMaze?.HeuristicsResults?.ShortestPathResult;
                var graph = shortestPathResult?.Graph;
                var startPoint = gameState.CurrentMaze?.MazeJumper?.StartPoint;
                
                if (shortestPathResult != null && graph != null && startPoint != null)
                {
                    var fallbackPath = PathResult.FromShortestPathResult(shortestPathResult, graph, startPoint, 0);
                    paths.Add(fallbackPath);
                }
                return paths;
            }
            
            if (gameState.VisualizationSettings.ShowAllPathsSimultaneously)
            {
                // Return all paths
                return gameState.AllPaths.ToList();
            }
            else
            {
                // Return only the current path
                var currentPath = gameState.CurrentPath;
                if (currentPath != null)
                {
                    paths.Add(currentPath);
                }
                return paths;
            }
        }

        /// <summary>
        /// Renders the graph during animation mode, showing algorithm state.
        /// </summary>
        private void RenderAnimationStep(GameState gameState, Graph graph, IMazeJumper mazeJumper, 
            Dictionary<MazePoint, Vector2> nodePositions)
        {
            var step = gameState.AnimationController!.CurrentStep!;
            var visualSettings = gameState.VisualizationSettings;

            // Draw edges first
            var drawnEdges = new HashSet<(MazePoint, MazePoint)>();
            
            foreach (var kvp in graph.Nodes)
            {
                var fromPoint = kvp.Key;
                var node = kvp.Value;

                if (!nodePositions.ContainsKey(fromPoint)) continue;
                var fromPos = nodePositions[fromPoint];

                foreach (var edge in node.Edges)
                {
                    if (!nodePositions.ContainsKey(edge.Point)) continue;
                    
                    var reverseKey = (edge.Point, fromPoint);
                    if (drawnEdges.Contains(reverseKey)) continue;
                    
                    var toPos = nodePositions[edge.Point];
                    var edgeKey = (fromPoint, edge.Point);
                    drawnEdges.Add(edgeKey);

                    // Check if this is the currently examined edge
                    bool isCurrentEdge = step.CurrentEdge.HasValue && 
                        ((step.CurrentEdge.Value.From.Equals(fromPoint) && step.CurrentEdge.Value.To.Equals(edge.Point)) ||
                         (step.CurrentEdge.Value.From.Equals(edge.Point) && step.CurrentEdge.Value.To.Equals(fromPoint)));

                    Color edgeColor;
                    float edgeWidth;

                    if (isCurrentEdge)
                    {
                        // Highlight current edge being examined
                        if (step.Type == StepType.RelaxEdge)
                        {
                            edgeColor = visualSettings.RelaxedEdgeColor;
                        }
                        else
                        {
                            edgeColor = visualSettings.ExaminedEdgeColor;
                        }
                        edgeWidth = 4f;
                    }
                    else
                    {
                        edgeColor = new Color(0.5f, 0.5f, 0.5f);
                        edgeWidth = 1.5f;
                    }
                    
                    DrawEdgeWithColor(fromPos, toPos, edge.DirectionsToPoint.Length, edgeColor, edgeWidth, false, false);
                }
            }

            // Draw nodes on top
            foreach (var kvp in graph.Nodes)
            {
                var point = kvp.Key;
                var node = kvp.Value;

                if (!nodePositions.ContainsKey(point)) continue;
                var pos = nodePositions[point];

                // Determine node color based on algorithm state
                Color nodeColor;
                float radius = 28f;

                if (point.Equals(mazeJumper.StartPoint))
                {
                    nodeColor = new Color(0.2f, 0.8f, 0.2f);
                    radius = 35f;
                }
                else if (point.Equals(mazeJumper.EndPoint))
                {
                    nodeColor = new Color(0.9f, 0.2f, 0.2f);
                    radius = 35f;
                }
                else if (step.CurrentNode?.Equals(point) == true)
                {
                    nodeColor = visualSettings.CurrentNodeColor;
                    radius = 32f;
                }
                else if (step.VisitedNodes.Contains(point))
                {
                    nodeColor = visualSettings.VisitedNodeColor;
                }
                else if (step.FrontierNodes.Contains(point))
                {
                    nodeColor = visualSettings.FrontierNodeColor;
                }
                else
                {
                    nodeColor = new Color(0.3f, 0.3f, 0.3f); // Unvisited
                }

                var circle = CreateCirclePolygon(pos, radius, nodeColor);
                circle.ZIndex = 2;
                _mazeGrid.AddChild(circle);

                // Draw distance label (use current step's distances)
                int distance = step.Distances.GetValueOrDefault(point, int.MaxValue);
                var distanceLabel = new Label();
                distanceLabel.Text = distance == int.MaxValue ? "inf" : distance.ToString();
                distanceLabel.HorizontalAlignment = HorizontalAlignment.Center;
                distanceLabel.VerticalAlignment = VerticalAlignment.Center;
                float labelWidth = distance >= 100 ? 36 : (distance >= 10 || distance == int.MaxValue ? 24 : 16);
                distanceLabel.Position = pos - new Vector2(labelWidth / 2, 10);
                distanceLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
                distanceLabel.AddThemeFontSizeOverride("font_size", distance == int.MaxValue ? 14 : 18);
                distanceLabel.ZIndex = 4;
                _mazeGrid.AddChild(distanceLabel);

                // Draw coordinate label below the node
                var coordLabel = new Label();
                coordLabel.Text = $"({point.X},{point.Y})";
                if (point.Z != 0) coordLabel.Text = $"({point.X},{point.Y},{point.Z})";
                coordLabel.Position = pos + new Vector2(-25, radius + 4);
                coordLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
                coordLabel.AddThemeFontSizeOverride("font_size", 14);
                coordLabel.ZIndex = 3;
                _mazeGrid.AddChild(coordLabel);
            }
        }

        #region Layout Algorithm

        /// <summary>
        /// Grid-aware layout: positions based on actual maze coordinates.
        /// </summary>
        /// <param name="graph">The graph to layout</param>
        /// <param name="mazeJumper">Maze jumper for size info</param>
        private Dictionary<MazePoint, Vector2> CalculateGridAwareLayout(Graph graph, IMazeJumper mazeJumper)
        {
            var positions = new Dictionary<MazePoint, Vector2>();
            
            const float cellSpacing = 90f;

            foreach (var kvp in graph.Nodes)
            {
                var point = kvp.Key;

                // Use maze coordinates directly, flip Y for screen
                float x = point.X * cellSpacing;
                float y = (mazeJumper.Size.Y - 1 - point.Y) * cellSpacing;
                
                // Add small offset for Z levels (3D mazes)
                if (point.Z != 0)
                {
                    x += point.Z * 10f;
                    y += point.Z * 10f;
                }

                positions[point] = new Vector2(x, y);
            }

            return positions;
        }

        #endregion

        #region Helper Methods

        private HashSet<MazePoint> GetNodesOnShortestPath(Graph graph, MazePoint startPoint, List<Direction> pathDirections)
        {
            var nodesOnPath = new HashSet<MazePoint> { startPoint };
            var currentPoint = startPoint;

            foreach (var direction in pathDirections)
            {
                currentPoint = MovePoint(currentPoint, direction);
                if (graph.Nodes.ContainsKey(currentPoint))
                {
                    nodesOnPath.Add(currentPoint);
                }
            }

            return nodesOnPath;
        }

        private static MazePoint MovePoint(MazePoint point, Direction direction)
        {
            return direction switch
            {
                Direction.Left => new MazePoint(point.X - 1, point.Y, point.Z),
                Direction.Right => new MazePoint(point.X + 1, point.Y, point.Z),
                Direction.Forward => new MazePoint(point.X, point.Y + 1, point.Z),
                Direction.Back => new MazePoint(point.X, point.Y - 1, point.Z),
                Direction.Up => new MazePoint(point.X, point.Y, point.Z + 1),
                Direction.Down => new MazePoint(point.X, point.Y, point.Z - 1),
                _ => point
            };
        }

        private static bool IsEdgeOnShortestPath(MazePoint from, MazePoint to, HashSet<MazePoint> shortestPathNodes)
        {
            return shortestPathNodes.Contains(from) && shortestPathNodes.Contains(to);
        }

        /// <summary>
        /// Gets directed edges (from, to) that are on the shortest path.
        /// </summary>
        private HashSet<(MazePoint from, MazePoint to)> GetShortestPathEdges(Graph graph, MazePoint startPoint, List<Direction> pathDirections)
        {
            var edges = new HashSet<(MazePoint, MazePoint)>();
            var currentPoint = startPoint;
            MazePoint? lastJunction = graph.Nodes.ContainsKey(startPoint) ? startPoint : null;

            foreach (var direction in pathDirections)
            {
                var nextPoint = MovePoint(currentPoint, direction);
                
                // If we've reached a junction, record the edge from the last junction
                if (graph.Nodes.ContainsKey(nextPoint) && lastJunction != null)
                {
                    edges.Add((lastJunction, nextPoint));
                    lastJunction = nextPoint;
                }
                
                currentPoint = nextPoint;
            }

            return edges;
        }

        #endregion

        #region Drawing Methods

        private void DrawEdgeWithColor(Vector2 fromPos, Vector2 toPos, int weight, Color color, float width, bool showArrowToTarget = false, bool showArrowToSource = false)
        {
            var line = new Line2D();
            line.AddPoint(fromPos);
            line.AddPoint(toPos);
            line.Width = width;
            line.DefaultColor = color;
            line.ZIndex = 0;
            _mazeGrid.AddChild(line);

            var midpoint = (fromPos + toPos) / 2;
            var weightLabel = new Label();
            weightLabel.Text = weight.ToString();
            weightLabel.Position = midpoint - new Vector2(12, 12);
            
            // Use edge color for weight label with slight brightness adjustment
            var labelColor = new Color(
                Math.Min(1f, color.R + 0.2f),
                Math.Min(1f, color.G + 0.2f),
                Math.Min(1f, color.B + 0.2f)
            );
            weightLabel.AddThemeColorOverride("font_color", labelColor);
            weightLabel.AddThemeFontSizeOverride("font_size", 16);
            weightLabel.ZIndex = 1;
            _mazeGrid.AddChild(weightLabel);

            if (showArrowToTarget)
            {
                DrawArrow(fromPos, toPos, color);
            }
            else if (showArrowToSource)
            {
                DrawArrow(toPos, fromPos, color);
            }
        }

        private void DrawEdge(Vector2 fromPos, Vector2 toPos, int weight, bool isOnShortestPath, bool showArrowToTarget = false, bool showArrowToSource = false)
        {
            var color = isOnShortestPath ? new Color(0.2f, 0.6f, 1.0f) : new Color(0.5f, 0.5f, 0.5f);
            var width = isOnShortestPath ? 3f : 1.5f;
            DrawEdgeWithColor(fromPos, toPos, weight, color, width, showArrowToTarget, showArrowToSource);
        }

        private void DrawArrow(Vector2 fromPos, Vector2 toPos, Color color)
        {
            // Calculate arrow position (75% along the edge, toward target)
            var direction = (toPos - fromPos).Normalized();
            var arrowPos = fromPos + (toPos - fromPos) * 0.7f;
            
            // Arrow head dimensions
            const float arrowSize = 12f;
            const float arrowAngle = 25f * (float)Math.PI / 180f; // 25 degrees
            
            // Calculate arrow head points
            var perpendicular = new Vector2(-direction.Y, direction.X);
            var backOffset = -direction * arrowSize;
            
            var leftPoint = arrowPos + backOffset + perpendicular * arrowSize * (float)Math.Tan(arrowAngle);
            var rightPoint = arrowPos + backOffset - perpendicular * arrowSize * (float)Math.Tan(arrowAngle);
            
            // Draw filled arrow triangle
            var arrow = new Polygon2D();
            arrow.Polygon = new Vector2[] { arrowPos, leftPoint, rightPoint };
            arrow.Color = color;
            arrow.ZIndex = 1;
            _mazeGrid.AddChild(arrow);
        }

        private void DrawNode(Vector2 pos, MazePoint point, int distance, MazePoint startPoint, MazePoint endPoint, bool isOnShortestPath)
        {
            DrawNodeWithPaths(pos, point, distance, startPoint, endPoint, isOnShortestPath, new List<int> { 0 }, new PathVisualizationSettings());
        }

        private void DrawNodeWithPaths(Vector2 pos, MazePoint point, int distance, MazePoint startPoint, MazePoint endPoint, bool isOnPath, List<int> pathIndices, PathVisualizationSettings visualSettings, int edgeCount = 0)
        {
            Color nodeColor;
            float radius = 28f;
            bool isJunction = edgeCount > 2;

            if (point.Equals(startPoint))
            {
                nodeColor = new Color(0.2f, 0.8f, 0.2f);
                radius = 35f;
            }
            else if (point.Equals(endPoint))
            {
                nodeColor = new Color(0.9f, 0.2f, 0.2f);
                radius = 35f;
            }
            else if (isOnPath && pathIndices.Count > 0)
            {
                // Use the color from the lowest path index (optimal path takes precedence)
                int primaryPathIndex = pathIndices.Min();
                nodeColor = visualSettings.GetPathColor(primaryPathIndex);
                
                // Scale up decision points if enabled
                if (isJunction && visualSettings.DecisionDetailLevel != DecisionDetailLevel.Off)
                {
                    radius *= visualSettings.DecisionPointScale;
                }
            }
            else
            {
                nodeColor = new Color(1.0f, 0.6f, 0.2f);
            }

            var circle = CreateCirclePolygon(pos, radius, nodeColor);
            circle.ZIndex = 2;
            _mazeGrid.AddChild(circle);

            // Draw distance label inside the node
            var distanceLabel = new Label();
            distanceLabel.Text = distance == int.MaxValue ? "∞" : distance.ToString();
            distanceLabel.HorizontalAlignment = HorizontalAlignment.Center;
            distanceLabel.VerticalAlignment = VerticalAlignment.Center;
            // Center the label on the node
            float labelWidth = distance == int.MaxValue ? 24 : (distance >= 100 ? 36 : (distance >= 10 ? 24 : 16));
            distanceLabel.Position = pos - new Vector2(labelWidth / 2, 10);
            distanceLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            distanceLabel.AddThemeFontSizeOverride("font_size", 18);
            distanceLabel.ZIndex = 4;
            _mazeGrid.AddChild(distanceLabel);

            // Draw coordinate label below the node
            var coordLabel = new Label();
            coordLabel.Text = $"({point.X},{point.Y})";
            if (point.Z != 0) coordLabel.Text = $"({point.X},{point.Y},{point.Z})";
            coordLabel.Position = pos + new Vector2(-25, radius + 4);
            coordLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            coordLabel.AddThemeFontSizeOverride("font_size", 14);
            coordLabel.ZIndex = 3;
            _mazeGrid.AddChild(coordLabel);

            // Draw decision point badge if enabled
            if (isJunction && visualSettings.DecisionDetailLevel != DecisionDetailLevel.Off)
            {
                DrawDecisionPointBadge(pos, radius, edgeCount, visualSettings);
            }
        }

        private void DrawDecisionPointBadge(Vector2 pos, float nodeRadius, int edgeCount, PathVisualizationSettings visualSettings)
        {
            // Draw a small badge showing the branching factor
            float badgeRadius = 12f;
            var badgePos = pos + new Vector2(nodeRadius * 0.7f, -nodeRadius * 0.7f);
            
            var badge = CreateCirclePolygon(badgePos, badgeRadius, visualSettings.DecisionPointColor);
            badge.ZIndex = 5;
            _mazeGrid.AddChild(badge);
            
            var badgeLabel = new Label();
            badgeLabel.Text = edgeCount.ToString();
            badgeLabel.Position = badgePos - new Vector2(5, 8);
            badgeLabel.AddThemeColorOverride("font_color", new Color(0f, 0f, 0f));
            badgeLabel.AddThemeFontSizeOverride("font_size", 12);
            badgeLabel.ZIndex = 6;
            _mazeGrid.AddChild(badgeLabel);
        }

        private static Polygon2D CreateCirclePolygon(Vector2 center, float radius, Color color)
        {
            var polygon = new Polygon2D();
            var points = new List<Vector2>();
            const int segments = 24;

            for (int i = 0; i < segments; i++)
            {
                float angle = (float)(i * 2 * Math.PI / segments);
                points.Add(center + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                ));
            }

            polygon.Polygon = points.ToArray();
            polygon.Color = color;
            return polygon;
        }

        #endregion
    }
}
