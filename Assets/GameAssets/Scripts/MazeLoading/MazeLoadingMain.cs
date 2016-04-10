using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Examples.UI;
using Assets.GameAssets.Scripts.Maze;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.MazeUI;
using Assets.GameAssets.Scripts.UI;
using Assets.GameAssets.Scripts.UI.Helper;
using Assets.GameAssets.Scripts.UI.Menu.Settings;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Assets.GameAssets.Scripts.MazeLoading
{
    public class MazeLoadingMain : MonoBehaviour
    {
        public RectTransform leftPanel;
        public RectTransform rightPanel;
        public Text validationResult;
        public Button restartButton;
        public Button startButton;

        private ICurrentSettingsHolder _currentSettingsHolder;
        private IMazeGenerationFactory _generationFactory;
        private IMazeValidator _validator;
        private ICurrentMazeHolder _currentMazeHolder;
        private ISceneLoader _sceneLoader;
        private IMazeNeedsGenerating _mazeNeedsGenerating;
        private IResourceLoader _resourceLoader;
        private IUiModeSwitcher _uiModeSwitcher;

        [PostInject]
        public void Init(IMazeGenerationFactory generationFactory, ICurrentSettingsHolder currentSettingsHolder, IMazeValidator validator, ICurrentMazeHolder currentMazeHolder, ISceneLoader sceneLoader, IMazeNeedsGenerating mazeNeedsGenerating, IGenerateTestCase generateTestCase, IResourceLoader resourceLoader, IUiModeSwitcher uiModeSwitcher)
        {
            _generationFactory = generationFactory;
            _currentSettingsHolder = currentSettingsHolder;
            _validator = validator;
            _currentMazeHolder = currentMazeHolder;
            _sceneLoader = sceneLoader;
            _resourceLoader = resourceLoader;
            _mazeNeedsGenerating = mazeNeedsGenerating;
            _uiModeSwitcher = uiModeSwitcher;
        }

        public void Awake()
        {
            var controlStrings = new List<string>()
            {
                "Controls",
                "WASD: Move the camera",
                "Z: Zoom In",
                "X: Zoom Out",
                "C: Remove Dead Ends",
                "ESC: Return to Menu",
                "V: Return to Menu and Regenerate Maze",
                "F: Toggle UI Mode between Shortest Path, Paths without Dead End"
            };
            foreach (var controlString in controlStrings)
            {
                _resourceLoader.InstantiateControl<TextControl>(rightPanel).Initialize(controlString);
            }
            
            
            var heuristicsStrings = new List<String>()
            {
                "Heuristics",
            };
            foreach (var heuristicsString in heuristicsStrings)
            {
                _resourceLoader.InstantiateControl<TextControl>(leftPanel).Initialize(heuristicsString);
            }
            


            startButton.enabled = false;
            startButton.onClick.AddListener(StartButtonOnClick);
            restartButton.onClick.AddListener(RestartButtonOnClick);
            validationResult.text = "";
            if (_mazeNeedsGenerating.Generate)
            { 
                _currentMazeHolder.Results = _generationFactory.GenerateMaze(_currentSettingsHolder.Settings);
                //var validation = _validator.EveryPointHasDirection(_currentMazeHolder.MazeJumper);
                _mazeNeedsGenerating.Generate = false;
            }
            startButton.enabled = true;
        }

        private void StartButtonOnClick()
        {
            _sceneLoader.LoadMaze();
        }

        private void RestartButtonOnClick()
        {
            _sceneLoader.LoadMenu();
        }
    }
}
