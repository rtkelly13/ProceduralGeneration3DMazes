using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Examples.UI;
using Assets.GameAssets.Scripts.Maze;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;
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
        public Button regenerateButton;

        private MazeGenerationResults _results;

        private ICurrentSettingsHolder _currentSettingsHolder;
        private IMazeGenerationFactory _generationFactory;
        private IMazeValidator _validator;
        private ICurrentMazeHolder _currentMazeHolder;
        private ISceneLoader _sceneLoader;
        private IMazeNeedsGenerating _mazeNeedsGenerating;
        private IResourceLoader _resourceLoader;
        private IUiModeSwitcher _uiModeSwitcher;
        private ITimeRecorder _timeRecorder;

        [PostInject]
        public void Init(
            IMazeGenerationFactory generationFactory,
            ICurrentSettingsHolder currentSettingsHolder, 
            IMazeValidator validator,
            ICurrentMazeHolder currentMazeHolder,
            ISceneLoader sceneLoader,
            IMazeNeedsGenerating mazeNeedsGenerating, 
            IGenerateTestCase generateTestCase, 
            IResourceLoader resourceLoader,
            IUiModeSwitcher uiModeSwitcher,
            ITimeRecorder timeRecorder)
        {
            _generationFactory = generationFactory;
            _currentSettingsHolder = currentSettingsHolder;
            _validator = validator;
            _currentMazeHolder = currentMazeHolder;
            _sceneLoader = sceneLoader;
            _resourceLoader = resourceLoader;
            _mazeNeedsGenerating = mazeNeedsGenerating;
            _uiModeSwitcher = uiModeSwitcher;
            _timeRecorder = timeRecorder;
        }

        public void Awake()
        {
            startButton.enabled = false;
            startButton.onClick.AddListener(StartButtonOnClick);
            restartButton.onClick.AddListener(RestartButtonOnClick);
            regenerateButton.onClick.AddListener(RegenerateMaze);
            validationResult.text = "";
            
            ReloadUiAndRegenerateMaze();

            startButton.enabled = true;
        }

        private void ReloadUiAndRegenerateMaze()
        {
            startButton.enabled = false;
            restartButton.enabled = false;
            regenerateButton.enabled = false;

            if (_mazeNeedsGenerating.Generate)
            {
                _results = _generationFactory.GenerateMaze(_currentSettingsHolder.Settings);
                _currentMazeHolder.Results = _results;
                //var validation = _validator.EveryPointHasDirection(_currentMazeHolder.MazeJumper);
                _mazeNeedsGenerating.Generate = false;
            }
            else
            {
                _results = _currentMazeHolder.Results;
            }

            leftPanel.Clear();
            rightPanel.Clear();
            var controlStrings = new List<string>()
            {
                "Controls",
                "WASD: Move the camera",
                "Z: Zoom in",
                "X: Zoom out",
                "C: Remove dead ends",
                "ESC: Return to menu",
                "V: Return to menu and regenerate maze",
                "F: Toggle UI mode between shortest path, paths without dead ends, Agent pfath"
            };
            foreach (var controlString in controlStrings)
            {
                _resourceLoader.InstantiateControl<TextControl>(rightPanel).Initialize(controlString);
            }

            var maximumUse = _results.HeuristicsResults.MazeStats.Standard.MaximumUse;
            var minumumUse = _results.HeuristicsResults.MazeStats.Standard.MinimumUse;
            var heuristicsStrings = new List<String>()
            {
                "Heuristics",
                String.Format("Total cells: {0}", _results.HeuristicsResults.TotalCells),
                String.Format("Cells down dead end: {0}", _results.DeadEndFillerResults.TotalCellsFilledIn),
                String.Format("Shortest path: {0}", _results.HeuristicsResults.ShortestPathResult.ShortestPath),
                String.Format("Model Generation Time: {0}", _timeRecorder.GetStringFromTime(_results.ModelTime)),
                String.Format("Maze Generation Time: {0}", _timeRecorder.GetStringFromTime(_results.GenerationTime)),
                String.Format("Dead End Filling Time: {0}", _timeRecorder.GetStringFromTime(_results.DeadEndFillerTime)),
                String.Format("Agent Run Time: {0}", _timeRecorder.GetStringFromTime(_results.AgentGenerationTime)),
                String.Format("Heauristics Time: {0}", _timeRecorder.GetStringFromTime(_results.HeuristicsTime)),
                String.Format("Total Time: {0}", _timeRecorder.GetStringFromTime(_results.TotalTime)),
            };
            foreach (var heuristicsString in heuristicsStrings)
            {
                _resourceLoader.InstantiateControl<TextControl>(leftPanel).Initialize(heuristicsString);
            }

            startButton.enabled = true;
            restartButton.enabled = true;
            regenerateButton.enabled = true;
        }


        private void RegenerateMaze()
        {
            _mazeNeedsGenerating.Generate = true;
            ReloadUiAndRegenerateMaze();
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
