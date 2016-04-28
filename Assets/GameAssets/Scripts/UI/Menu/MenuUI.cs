using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Examples.UI;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.MazeLoading;
using Assets.GameAssets.Scripts.UI.Controls;
using Assets.GameAssets.Scripts.UI.Helper;
using Assets.GameAssets.Scripts.UI.Menu.Settings;
using Assets.GameAssets.Scripts.UI.Menu.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Assets.GameAssets.Scripts.UI.Menu
{
    public class MenuUI : MonoBehaviour
    {
        public RectTransform leftPanel;
        public RectTransform rightPanelSettings;
        public Text validationResult;
        public Button button;

        private IAlgorithmsProvider _algorithmsProvider;
        private IResourceLoader _resourceLoader;
        private ISettingsBuilder _settingsBuilder;
        private IAlgorithmSettingsInitialiser _algorithmSettingsInitialiser;
        private IValidateSettings _validateSettings;
        private IModelOptionsProvider _modelOptionsProvider;
        private ICurrentSettingsHolder _currentSettingsHolder;
        private IWallCarverOptionsProvider _wallCarverOptionsProvider;
        private ISceneLoader _sceneLoader;
        private IYesNoOptionsProvider _yesNoOptionsProvider;
        private IMazeNeedsGenerating _mazeNeedsGenerating;
        private IAgentOptionsProvider _agentOptionsProvider;

        [PostInject]
        public void Init(IAlgorithmsProvider algorithmsProvider,
            IResourceLoader resourceLoader, 
            ISettingsBuilder settingsBuilder, 
            IAlgorithmSettingsInitialiser algorithmSettingsInitialiser,
            IValidateSettings validateSettings, 
            IModelOptionsProvider modelOptionsProvider,
            ICurrentSettingsHolder currentSettingsHolder,  
            ISceneLoader sceneLoader, 
            IWallCarverOptionsProvider wallCarverOptionsProvider,
            IYesNoOptionsProvider yesNoOptionsProvider,
            IMazeNeedsGenerating mazeNeedsGenerating,
            IAgentOptionsProvider agentOptionsProvider)
        {
            _algorithmsProvider = algorithmsProvider;
            _resourceLoader = resourceLoader;
            _settingsBuilder = settingsBuilder;
            _algorithmSettingsInitialiser = algorithmSettingsInitialiser;
            _validateSettings = validateSettings;
            _modelOptionsProvider = modelOptionsProvider;
            _currentSettingsHolder = currentSettingsHolder;
            _sceneLoader = sceneLoader;
            _wallCarverOptionsProvider = wallCarverOptionsProvider;
            _yesNoOptionsProvider = yesNoOptionsProvider;
            _mazeNeedsGenerating = mazeNeedsGenerating;
            _agentOptionsProvider = agentOptionsProvider;
        }
        // Use this for initialization
        void Start ()
        {

        }

        public void OnClick()
        {
            _mazeNeedsGenerating.Generate = true;
            _sceneLoader.LoadMazeLoader();
        }

        void Awake()
        {

            _currentSettingsHolder.Settings  = _currentSettingsHolder.Settings ?? new MazeGenerationSettings
            {
                Size = new MazeSize
                {
                    X = 1,
                    Y = 1,
                    Z = 1
                },
                Option = MazeType.None,
                Algorithm = Algorithm.None
            };

            _resourceLoader.InstantiateControl<DropdownControl>(leftPanel).Initialise("Algorithms", _algorithmsProvider.DropdownOptions, 
                _algorithmsProvider.DropdownOptions.FindIndex(x => x.Value == _currentSettingsHolder.Settings.Algorithm), true, InitialiseRightPanel);

            _resourceLoader.InstantiateControl<SliderControl>(leftPanel).Initialize("X", 1, 75, _currentSettingsHolder.Settings.Size.X, i =>
            {
                _currentSettingsHolder.Settings.Size.X = i;
                Validate();
            });

            _resourceLoader.InstantiateControl<SliderControl>(leftPanel).Initialize("Y", 1, 75, _currentSettingsHolder.Settings.Size.Y, i =>
            {
                _currentSettingsHolder.Settings.Size.Y = i;
                Validate();
            });

            _resourceLoader.InstantiateControl<SliderControl>(leftPanel).Initialize("Z", 1, 75, _currentSettingsHolder.Settings.Size.Z, i =>
            {
                _currentSettingsHolder.Settings.Size.Z = i;
                Validate();
            });

            _resourceLoader.InstantiateControl<DropdownControl>(leftPanel)
                .Initialise("Model option", _modelOptionsProvider.DropdownOptions,
                _modelOptionsProvider.DropdownOptions.FindIndex(x => x.Value == _currentSettingsHolder.Settings.Option), true,
                    option => {
                        _currentSettingsHolder.Settings.Option = option;
                        Validate();
                    });

            _resourceLoader.InstantiateControl<DropdownControl>(leftPanel)
                .Initialise("Carve extra walls", _wallCarverOptionsProvider.DropdownOptions, 
                _wallCarverOptionsProvider.DropdownOptions.FindIndex(x => x.Value == _currentSettingsHolder.Settings.ExtraWalls), true,
                    option =>
                    {
                        _currentSettingsHolder.Settings.ExtraWalls =
                            _wallCarverOptionsProvider.DropdownOptions.Single(x => x.Key == option).Value;
                        Validate();
                    });

            _resourceLoader.InstantiateControl<DropdownControl>(leftPanel)
                .Initialise("Force doors at edge of maze", _yesNoOptionsProvider.DropdownOptions, 
                _yesNoOptionsProvider.DropdownOptions.FindIndex(x => x.Value == _currentSettingsHolder.Settings.DoorsAtEdge), true,
                    option =>
                    {
                        _currentSettingsHolder.Settings.DoorsAtEdge =
                            _yesNoOptionsProvider.DropdownOptions.Single(x => x.Key == option).Value;
                        Validate();
                    });

            _resourceLoader.InstantiateControl<DropdownControl>(leftPanel)
                .Initialise("Agent to run", _agentOptionsProvider.DropdownOptions, 
                _agentOptionsProvider.DropdownOptions.FindIndex(x => x.Value == _currentSettingsHolder.Settings.AgentType), true, option =>
                {
                    _currentSettingsHolder.Settings.AgentType =
                            _agentOptionsProvider.DropdownOptions.Single(x => x.Key == option).Value;
                    Validate();
                });

            button.enabled = false;
            button.onClick.AddListener(OnClick);
        }

        private void InitialiseRightPanel(Algorithm algorithm)
        {
            _currentSettingsHolder.Settings.Algorithm = algorithm;
            Validate();
            _settingsBuilder.BuildMenu(rightPanelSettings, algorithm, _currentSettingsHolder.Settings, settings =>
            {
                //Settings are updated here from built menu
                _algorithmSettingsInitialiser.InitialiseOver(settings, _currentSettingsHolder.Settings);
                _currentSettingsHolder.Settings = settings;
                Validate();
            });
        }

        private void Validate()
        {
            var validation = _validateSettings.ValidateAndProcess(_currentSettingsHolder.Settings);
            validationResult.text = validation.IsValid ? "" : validation.Reason;
            button.enabled = validation.IsValid;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void Initialize()
        {
            
        }
    }
} 