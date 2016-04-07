using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Examples.UI;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI.Helper;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings.GrowingTree
{
    public class GrowingTreeSettingsBuilder : ISettingBuilder
    {
        public Algorithm AlgorithmType { get; private set; }

        private readonly IResourceLoader _resourceLoader;
        private readonly IAlgorithmSettingsInitialiser _algorithmSettingsInitialiser;
        private readonly IGrowingTreeStrategyStorage _growingTreeStrategyStorage;
        private GrowingTreeSettings _growingTreeSettings;

        public GrowingTreeSettingsBuilder(IResourceLoader resourceLoader, IAlgorithmSettingsInitialiser algorithmSettingsInitialiser, IGrowingTreeStrategyStorage growingTreeStrategyStorage)
        {
            _resourceLoader = resourceLoader;
            _algorithmSettingsInitialiser = algorithmSettingsInitialiser;
            _growingTreeStrategyStorage = growingTreeStrategyStorage;
            AlgorithmType = Algorithm.GrowingTreeAlgorithm;
        }

        public MazeGenerationSettings GetSettings()
        {
            return _growingTreeSettings;
        }

        public void BuildMenu(Transform transform, MazeGenerationSettings existingSettings, Action<MazeGenerationSettings> settingsChanged)
        {
            _growingTreeSettings = existingSettings as GrowingTreeSettings ?? new GrowingTreeSettings();
            _growingTreeSettings.Strategies = _growingTreeSettings.Strategies ?? new List<GrowingTreeStrategy>();
            _algorithmSettingsInitialiser.InitialiseOver(_growingTreeSettings, existingSettings);
            _growingTreeStrategyStorage.LoadStrategies(_growingTreeSettings.Strategies);

            _resourceLoader.InstantiateControl<TextControl>(transform).Initialize("Growing Tree Algorithm Settings");

            _resourceLoader.InstantiateControl<TextControl>(transform).Initialize("Cell selection weighting: the action will be randomly chosen ");

            _resourceLoader.InstantiateControl<SliderControl>(transform)
                .Initialize("Oldest", 0, 10,
                _growingTreeStrategyStorage.Get(GrowingTreeStrategy.Oldest), 
                i => StrategyChanged(GrowingTreeStrategy.Oldest, i, settingsChanged));

            _resourceLoader.InstantiateControl<SliderControl>(transform)
                .Initialize("Newest", 0, 10,
                _growingTreeStrategyStorage.Get(GrowingTreeStrategy.Newest),
                i => StrategyChanged(GrowingTreeStrategy.Newest, i, settingsChanged));

            _resourceLoader.InstantiateControl<SliderControl>(transform)
                .Initialize("Middle", 0, 10,
                _growingTreeStrategyStorage.Get(GrowingTreeStrategy.Middle),
                i => StrategyChanged(GrowingTreeStrategy.Middle, i, settingsChanged));

            _resourceLoader.InstantiateControl<SliderControl>(transform)
                .Initialize("Randomness", 0, 10,
                _growingTreeStrategyStorage.Get(GrowingTreeStrategy.Random),
                i => StrategyChanged(GrowingTreeStrategy.Random, i, settingsChanged));

            _resourceLoader.InstantiateControl<SliderControl>(transform)
                .Initialize("Random Oldest Half", 0, 10,
                _growingTreeStrategyStorage.Get(GrowingTreeStrategy.RandomOldest),
                i => StrategyChanged(GrowingTreeStrategy.RandomOldest, i, settingsChanged));

            _resourceLoader.InstantiateControl<SliderControl>(transform)
                .Initialize("Random Newest Half", 0, 10,
                _growingTreeStrategyStorage.Get(GrowingTreeStrategy.RandomNewest),
                i => StrategyChanged(GrowingTreeStrategy.RandomNewest, i, settingsChanged));

            settingsChanged(GetSettings());
        }

        private void StrategyChanged(GrowingTreeStrategy strategy, int sum, Action<MazeGenerationSettings> settingsChanged)
        {
            _growingTreeStrategyStorage.AddOrUpdate(strategy, sum);
            _growingTreeSettings.Strategies = _growingTreeStrategyStorage.GetAllStrategies().ToList();
            settingsChanged(GetSettings());
        }

        public void Dispose()
        {
            
        }
    }
}
