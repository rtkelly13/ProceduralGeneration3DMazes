using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Assets.GameAssets.Scripts.Maze;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.Maze.Solver;
using Assets.GameAssets.Scripts.MazeUI.ImageHandling;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class MazeUiBuilder : IMazeUiBuilder
    {
        private readonly ILoadCellPrefab _loadCellPrefab;
        private readonly IMazeHelper _mazeHelper;
        private readonly IDoorwayLoader _doorwayLoader;
        private readonly ILineDrawer _lineDrawer;
        private readonly ICellInformationProvider _cellInformation;
        private readonly ICircleLoader _circleLoader;
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;

        public MazeUiBuilder(
            ILoadCellPrefab loadCellPrefab,
            IMazeHelper mazeHelper,
            IDoorwayLoader doorwayLoader,
            ILineDrawer lineDrawer,
            ICellInformationProvider cellInformation,
            ICircleLoader circleLoader,
            IPointsAndDirectionsRetriever pointsAndDirectionsRetriever)
        {
            _loadCellPrefab = loadCellPrefab;
            _mazeHelper = mazeHelper;
            _doorwayLoader = doorwayLoader;
            _lineDrawer = lineDrawer;
            _cellInformation = cellInformation;
            _circleLoader = circleLoader;
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
        }

        public void BuildMazeUI(Transform mazeField, MazeGenerationResults results, int z, UiMode mode)
        {
            mazeField.Clear();
            var maze = results.MazeJumper;
            switch (mode)
            {
                case UiMode.ShortestPath:
                    maze.JumpToPoint(maze.StartPoint);
                    foreach (var direction in results.HeuristicsResults.ShortestPathResult.ShortestPathDirections)
                    {
                        _lineDrawer.DrawLine(mazeField, maze.CurrentPoint, direction, z, LineColour.Yellow);
                        maze.MoveInDirection(direction);
                    }
                    break;
                case UiMode.DeadEndLess:
                    foreach (var node in results.HeuristicsResults.ShortestPathResult.Graph.Nodes)
                    {
                        var point = node.Key;
                        if (point.Z == z && !_pointsAndDirectionsRetriever.PointIsStartOrEnd(point, results.MazeJumper.StartPoint, results.MazeJumper.EndPoint))
                        {
                            BuildAndAttachCircle(mazeField, "blue", point.X, point.Y);
                        }
                        maze.JumpToPoint(point);
                        foreach (var edge in node.Value.Edges)
                        {
                            maze.JumpToPoint(node.Key);
                            foreach (var direction in edge.DirectionsToPoint)
                            {
                                _lineDrawer.DrawLine(mazeField, maze.CurrentPoint, direction, z, LineColour.Blue);
                                maze.MoveInDirection(direction);
                            }
                        }
                    }
                    break;
                case UiMode.Agent:
                    foreach (var directionAndPoint in results.AgentResults.Movements)
                    {
                        maze.JumpToPoint(directionAndPoint.MazePoint);
                        _lineDrawer.DrawLine(mazeField, maze.CurrentPoint, directionAndPoint.Direction, z, LineColour.Green);
                        maze.MoveInDirection(directionAndPoint.Direction);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var points = _mazeHelper.GetForEachZ(maze.Size, z, x => x);
            foreach (var point in points)
            {
                maze.JumpToPoint(point);
                var directions = maze.GetFlagFromPoint();
                BuildAndAttachCell(mazeField, directions, point.X, point.Y);
            }
            if (maze.StartPoint.Z == z)
            {
                BuildAndAttachDoor(mazeField, DoorType.Entrance, maze.StartPoint.X, maze.StartPoint.Y);
            }
            if (maze.EndPoint.Z == z)
            {
                BuildAndAttachDoor(mazeField, DoorType.Exit, maze.EndPoint.X, maze.EndPoint.Y);
            }
        }

        private void BuildAndAttachDoor(Transform mazeField, DoorType type, int x, int y)
        {
            var prefab = _doorwayLoader.GetDoor(type);
            prefab.name = Enum.GetName(typeof (DoorType), type);
            prefab.transform.SetParent(mazeField, false);
            prefab.transform.localPosition = new Vector3(_cellInformation.CellSize*x, _cellInformation.CellSize*y, 0);
            prefab.transform.localRotation = Quaternion.identity;
            prefab.transform.localScale = Vector3.one;
        }

        private void BuildAndAttachCircle(Transform mazeField,string circleColour, int x, int y)
        {
            var prefab = _circleLoader.GetPrefab(circleColour);
            prefab.name = String.Format("NodeAt({0},{1})", x, y);
            prefab.transform.SetParent(mazeField, false);
            prefab.transform.localPosition = new Vector3(_cellInformation.CellSize * x, _cellInformation.CellSize * y, 0);
            prefab.transform.localRotation = Quaternion.identity;
            prefab.transform.localScale = Vector3.one;
        }

        private void BuildAndAttachCell(Transform mazeField, Direction d, int x, int y)
        {
            var prefab = _loadCellPrefab.GetPrefab(d);
            prefab.name = String.Format("Cell {0} {1}", x, y);
            prefab.transform.SetParent(mazeField, false);
            prefab.transform.localPosition = new Vector3(_cellInformation.CellSize*x, _cellInformation.CellSize*y, 0);
            prefab.transform.localRotation = Quaternion.identity;
            prefab.transform.localScale = Vector3.one;
        }
    }
}
