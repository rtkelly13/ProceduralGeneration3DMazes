using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public class LineDrawer : ILineDrawer
    {
        private readonly ILineLoader _lineLoader;
        private readonly ICellInformationProvider _cellInformation;

        public LineDrawer(ILineLoader lineLoader, ICellInformationProvider cellInformation)
        {
            _lineLoader = lineLoader;
            _cellInformation = cellInformation;
        }

        public void DrawLine(Transform mazeField, MazePoint p, Direction d, int z, LineColour colour)
        {
            var currentLevel = p.Z == z;
            switch (d)
            {
                case Direction.Left:
                    if (currentLevel)
                    {
                        DrawLeftLine(p, mazeField, colour);
                    }
                    break;
                case Direction.Right:
                    if (currentLevel)
                    {
                        DrawRightLine(p, mazeField, colour);
                    }
                    break;
                case Direction.Down:
                    if (currentLevel)
                    {
                        DrawDownLine(p, mazeField, colour);
                    }
                    if (p.Z - 1 == z)
                    {
                        DrawUpLine(p, mazeField, colour);
                    }
                    break;
                case Direction.Up:
                    if (currentLevel)
                    {
                        DrawUpLine(p, mazeField, colour);
                    }
                    if (p.Z + 1 == z)
                    {
                        DrawDownLine(p, mazeField, colour);
                    }
                    break;
                case Direction.Back:
                    if (currentLevel)
                    {
                        DrawBackLine(p, mazeField, colour);
                    }
                    break;
                case Direction.Forward:
                    if (currentLevel)
                    {
                        DrawForwardLine(p, mazeField, colour);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawUpLine(MazePoint p, Transform mazeField, LineColour colour)
        {
            var sprite = _lineLoader.GetLine(LineOption.HalfLine, colour);
            sprite.transform.SetParent(mazeField, false);
            sprite.transform.localPosition = new Vector3(_cellInformation.CellSize * p.X + _cellInformation.QuarterCellSize / 2, _cellInformation.CellSize * p.Y + +_cellInformation.QuarterCellSize / 2, -5);
            sprite.transform.localScale = new Vector3(_cellInformation.QuarterLineWidth * 3, _cellInformation.LineHeight, 0);
            sprite.transform.Rotate(0, 0, 45);
        }

        private void DrawDownLine(MazePoint p, Transform mazeField, LineColour colour)
        {
            var sprite = _lineLoader.GetLine(LineOption.HalfLine, colour);
            sprite.transform.SetParent(mazeField, false);
            sprite.transform.localPosition = new Vector3(_cellInformation.CellSize * p.X - _cellInformation.QuarterCellSize / 2, _cellInformation.CellSize * p.Y - _cellInformation.QuarterCellSize / 2, -5);
            sprite.transform.localScale = new Vector3(_cellInformation.QuarterLineWidth * 3, _cellInformation.LineHeight, 0);
            sprite.transform.Rotate(0, 0, 45);
        }

        private void DrawForwardLine(MazePoint p, Transform mazeField, LineColour colour)
        {
            var sprite = _lineLoader.GetLine(LineOption.FullLine, colour);
            sprite.transform.SetParent(mazeField, false);
            sprite.transform.localPosition = new Vector3(_cellInformation.CellSize * p.X, _cellInformation.CellSize * p.Y + _cellInformation.CellSize / 2, -5);
            sprite.transform.localScale = new Vector3(_cellInformation.LineWidth, _cellInformation.LineHeight, 0);
            sprite.transform.Rotate(0, 0, 90);
        }

        private void DrawBackLine(MazePoint p, Transform mazeField, LineColour colour)
        {
            var sprite = _lineLoader.GetLine(LineOption.FullLine, colour);
            sprite.transform.SetParent(mazeField, false);
            sprite.transform.localPosition = new Vector3(_cellInformation.CellSize * p.X, _cellInformation.CellSize * p.Y - _cellInformation.CellSize / 2, -5);
            sprite.transform.localScale = new Vector3(_cellInformation.LineWidth, _cellInformation.LineHeight, 0);
            sprite.transform.Rotate(0, 0, 90);
        }

        private void DrawLeftLine(MazePoint p, Transform mazeField, LineColour colour)
        {
            var sprite = _lineLoader.GetLine(LineOption.FullLine, colour);
            sprite.transform.SetParent(mazeField, false);
            sprite.transform.localPosition = new Vector3(_cellInformation.CellSize * p.X - _cellInformation.CellSize / 2, _cellInformation.CellSize * p.Y, -5);
            sprite.transform.localScale = new Vector3(_cellInformation.LineWidth, _cellInformation.LineHeight, 0);
        }

        private void DrawRightLine(MazePoint p, Transform mazeField, LineColour colour)
        {
            var sprite = _lineLoader.GetLine(LineOption.FullLine, colour);
            sprite.transform.SetParent(mazeField, false);
            sprite.transform.localPosition = new Vector3(_cellInformation.CellSize * p.X + _cellInformation.CellSize / 2, _cellInformation.CellSize * p.Y, -5);
            sprite.transform.localScale = new Vector3(_cellInformation.LineWidth, _cellInformation.LineHeight, 0);
        }
    }

    public enum LineColour
    {
        Yellow = 0,
        Blue = 1,
        Red = 2,
        Green = 3,
    }
}
