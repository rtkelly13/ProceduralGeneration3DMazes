using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Assets.GameAssets.Scripts.Maze;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.MazeUI.ImageHandling;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class MazeUiBuilder : IMazeUiBuilder
    {
        private readonly IMazePointFactory _mazePointFactory;
        private readonly ILoadCellPrefab _loadCellPrefab;
        private readonly IMazeHelper _mazeHelper;
        private readonly IDoorwayLoader _doorwayLoader;

        public MazeUiBuilder(IMazePointFactory mazePointFactory, ILoadCellPrefab loadCellPrefab, IMazeHelper mazeHelper, IDoorwayLoader doorwayLoader)
        {
            _mazePointFactory = mazePointFactory;
            _loadCellPrefab = loadCellPrefab;
            _mazeHelper = mazeHelper;
            _doorwayLoader = doorwayLoader;
        }

        public void BuildMazeUI(Transform mazeField, IMazeJumper maze, int z)
        {
            mazeField.Clear();
            var points =_mazeHelper.GetForEachZ(maze.Size, z, x => x);
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
            prefab.name = Enum.GetName(typeof(DoorType), type);
            prefab.transform.SetParent(mazeField, false);
            prefab.transform.localPosition = new Vector3(36 * x, 36 * y, 0);
            prefab.transform.localRotation = Quaternion.identity;
            prefab.transform.localScale = Vector3.one;
        }

        private void BuildAndAttachCell(Transform mazeField, Direction d, int x, int y)
        {
            var prefab = _loadCellPrefab.GetPrefab(d);
            prefab.name = String.Format("Cell {0} {1}", x, y);
            prefab.transform.SetParent(mazeField, false);
            prefab.transform.localPosition = new Vector3(36 * x, 36 * y, 0);
            prefab.transform.localRotation = Quaternion.identity;
            prefab.transform.localScale = Vector3.one;
        }
    }
}
