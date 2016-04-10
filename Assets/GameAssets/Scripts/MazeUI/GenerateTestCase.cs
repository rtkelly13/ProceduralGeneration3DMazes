using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.MazeLoading;
using Assets.GameAssets.Scripts.UI;
using Assets.GameAssets.Scripts.UI.Helper;
using Assets.GameAssets.Scripts.UI.Menu.Settings;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class GenerateTestCase : IGenerateTestCase
    {
        private readonly ICurrentSettingsHolder _currentSettingsHolder;
        private readonly IMazeGenerationFactory _generationFactory;
        private readonly IMazeValidator _validator;
        private readonly ICurrentMazeHolder _currentMazeHolder;

        public GenerateTestCase(ICurrentSettingsHolder currentSettingsHolder, IMazeGenerationFactory generationFactory, IMazeValidator validator, ICurrentMazeHolder currentMazeHolder)
        {
            _currentSettingsHolder = currentSettingsHolder;
            _generationFactory = generationFactory;
            _validator = validator;
            _currentMazeHolder = currentMazeHolder;
        }

        public void Run()
        {
            _currentSettingsHolder.Settings = new GrowingTreeSettings
            {
                Algorithm = Algorithm.GrowingTreeAlgorithm,
                Option = MazeType.DirectedMaze,
                ExtraWalls = WallCarverOption.Random,
                Size = new MazeSize
                {
                    X = 10,
                    Y = 10,
                    Z = 3
                },
                Strategies = new List<GrowingTreeStrategy>()
                {
                    GrowingTreeStrategy.Newest
                }
            };
            _currentMazeHolder.Results = _generationFactory.GenerateMaze(_currentSettingsHolder.Settings);
            //var validation = _validator.EveryPointHasDirection(_currentMazeHolder.MazeJumper);

        }
    }
}
