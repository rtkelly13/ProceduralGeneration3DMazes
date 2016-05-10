using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI;
using Assets.GameAssets.Scripts.UI.Helper;
using Ninject;
using Ninject.Modules;

namespace ProcGenMaze.Experiments
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var kernal = new StandardKernel())
            {
                var container = new Container();
                kernal.Load(new List<INinjectModule> { container });
                var runner = kernal.Get<IExperimentRunner>();
                runner.Run();
            }     
        }
    }
}
