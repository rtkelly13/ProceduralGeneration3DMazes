using System;
using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze
{
    public interface IMazeHelper
    {
        IEnumerable<T> GetForEachPoint<T>(MazeSize size, Func<MazePoint, T> function);
        IEnumerable<MazePoint> GetForEachPoint<T>(MazeSize size, Func<MazePoint, bool> func);
        IEnumerable<T> GetForEachZ<T>(MazeSize size, int z, Func<MazePoint, T> function);
        IEnumerable<MazePoint> GetForEachZ<T>(MazeSize size, int z, Func<MazePoint, bool> func);

        IEnumerable<MazePoint> GetPoints(MazeSize size, Func<MazePoint, bool> function); 

        void DoForEachPoint(MazeSize size, Action<MazePoint> action);
        void DoForEachZ(MazeSize size, int z, Action<MazePoint> action);
    }
}