using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.MazeLoading;
using Assets.GameAssets.Scripts.MazeUI.UserManagement;
using Assets.GameAssets.Scripts.UI;
using Assets.GameAssets.Scripts.UI.Helper;
using Assets.GameAssets.Scripts.UI.Menu.Settings;
using UnityEngine;
using Zenject;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class Main : MonoBehaviour
    {
        public Transform MazeField;
        public Transform CameraTransform;
        public Camera Camera;

        private MazeGenerationResults _mazeResults;

        private IMazeUiBuilder _mazeUiBuilder;
        private ICameraManagement _cameraManagement;
        private IInputHandler _inputHandler;
        private ICurrentMazeHolder _currentMazeHolder;
        private IGenerateTestCase _generateTestCase;
        private ISceneLoader _sceneLoader;
        private IMazeNeedsGenerating _mazeNeedsGenerating;
        private IModelStateHelper _modelStateHelper;
        private IUiModeSwitcher _uiModeSwitcher;

        [PostInject]
        public void Init(IMazeUiBuilder mazeUiBuilder, ICameraManagement cameraManagement, IInputHandler inputHandler, ICurrentMazeHolder currentMazeHolder, IGenerateTestCase generateTestCase, ISceneLoader sceneLoader, IMazeNeedsGenerating mazeNeedsGenerating, IModelStateHelper modelStateHelper, IUiModeSwitcher uiModeSwitcher)
        {
            _mazeUiBuilder = mazeUiBuilder;
            _cameraManagement = cameraManagement;
            _inputHandler = inputHandler;
            _currentMazeHolder = currentMazeHolder;
            _generateTestCase = generateTestCase;
            _mazeNeedsGenerating = mazeNeedsGenerating;
            _sceneLoader = sceneLoader;
            _modelStateHelper = modelStateHelper;
            _uiModeSwitcher = uiModeSwitcher;
        }

        private int _currentLevel = 0;
        private UiMode _currentMode = UiMode.ShortestPath;

        public void Awake()
        {
            _generateTestCase.Run();

            _mazeResults = _currentMazeHolder.Results;
            _cameraManagement.Init(CameraTransform, Camera, _mazeResults.MazeJumper);
            _inputHandler.Init(_cameraManagement, new InputHandlerOptions
            {
                MoveDown = () => {
                    if (_currentLevel > 0)
                    {
                        _currentLevel -= 1;
                        BuildUi();
                    }
                },
                MoveUp = () => {
                    if (_currentLevel < _mazeResults.MazeJumper.Size.Z - 1)
                    {
                        _currentLevel += 1;
                        BuildUi();
                    }
                },
                ToggleDeadEnds = () => {
                    _modelStateHelper.SetNextModelState(_mazeResults.MazeJumper);
                    BuildUi();
                },
                ReturnToMazeLoading = needsRegenerating =>
                {
                    _mazeNeedsGenerating.Generate = needsRegenerating;
                    _sceneLoader.LoadMazeLoader();
                },
                ToggleUI = () => {
                    _currentMode = _uiModeSwitcher.GetNext(_currentMode);
                    BuildUi();
                }
            });

            BuildUi();
        }

        private void BuildUi()
        {
            _mazeUiBuilder.BuildMazeUI(MazeField, _mazeResults, _currentLevel, _currentMode);
        }

        public void Update()
        {
            _inputHandler.HandleUpdate();
        }

    }
}
