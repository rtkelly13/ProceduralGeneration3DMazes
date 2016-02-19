using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Examples.UI;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.UI.Controls;
using Assets.GameAssets.Scripts.UI.Helper;
using Assets.GameAssets.Scripts.UI.Menu.Settings;
using Assets.GameAssets.Scripts.UI.Menu.Validation;
using UnityEngine;
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

        private AlgorithmSettings _currentSettings;

        private IAlgorithmsProvider _algorithmsProvider;
        private IResourceLoader _resourceLoader;
        private ISettingsBuilder _settingsBuilder;
        private IAlgorithmSettingsInitialiser _algorithmSettingsInitialiser;
        private IValidateSettings _validateSettings;
        private IModelOptionsProvider _modelOptionsProvider;

        public MenuUI()
        {
           _currentSettings = new AlgorithmSettings();
        }


        [PostInject]
        public void Init(IAlgorithmsProvider algorithmsProvider,
            IResourceLoader resourceLoader, 
            ISettingsBuilder settingsBuilder, 
            IAlgorithmSettingsInitialiser algorithmSettingsInitialiser,
            IValidateSettings validateSettings, 
            IModelOptionsProvider modelOptionsProvider)
        {
            _algorithmsProvider = algorithmsProvider;
            _resourceLoader = resourceLoader;
            _settingsBuilder = settingsBuilder;
            _algorithmSettingsInitialiser = algorithmSettingsInitialiser;
            _validateSettings = validateSettings;
            _modelOptionsProvider = modelOptionsProvider;
        }
        // Use this for initialization
        void Start ()
        {

        }

        void Awake()
        {
            _currentSettings = new AlgorithmSettings
            {
                Size = new MazeSize(),
                Option = ModelOption.None,
                Algorithm = Algorithm.None
            };

            _resourceLoader.InstantiateControl<DropdownControl>(leftPanel).Initialise("Algorithms", _algorithmsProvider.DropdownOptions, 0, true, InitialiseRightPanel);

            _resourceLoader.InstantiateControl<SliderControl>(leftPanel).Initialize("X", 0, 100, _currentSettings.Size.X, i =>
            {
                _currentSettings.Size.X = i;
                Validate();
            });

            _resourceLoader.InstantiateControl<SliderControl>(leftPanel).Initialize("Y", 0, 100, _currentSettings.Size.Y, i =>
            {
                _currentSettings.Size.Y = i;
                Validate();
            });

            _resourceLoader.InstantiateControl<SliderControl>(leftPanel).Initialize("Z", 0, 100, _currentSettings.Size.Y, i =>
            {
                _currentSettings.Size.Z = i;
                Validate();
            });

            _resourceLoader.InstantiateControl<DropdownControl>(leftPanel)
                .Initialise("Model Option", _modelOptionsProvider.DropdownOptions, 0, true,
                    option => {
                        _currentSettings.Option = option;
                        Validate();
                    });

            button.enabled = false;
            button.onClick.AddListener(delegate
            {
                //Do action on validated settings option.
                
            });
        }

        private void InitialiseRightPanel(Algorithm algorithm)
        {
            _currentSettings.Algorithm = algorithm;
            Validate();
            _settingsBuilder.BuildMenu(rightPanelSettings, algorithm, _currentSettings, settings =>
            {
                //Settings are updated here from built menu
                _algorithmSettingsInitialiser.InitialiseOver(settings, _currentSettings);
                _currentSettings = settings;
                Validate();
            });
        }

        private void Validate()
        {
            var validation = _validateSettings.ValidateAndProcess(_currentSettings);
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