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

        private IMazeUiBuilder _mazeUiBuilder;
        private IMazeJumper _maze;
        private ICameraManagement _cameraManagement;
        private IInputHandler _inputHandler;
        private ICurrentMazeHolder _currentMazeHolder;
        private IGenerateTestCase _generateTestCase;

        [PostInject]
        public void Init(IMazeUiBuilder mazeUiBuilder, ICameraManagement cameraManagement, IInputHandler inputHandler, ICurrentMazeHolder currentMazeHolder, IGenerateTestCase generateTestCase)
        {
            _mazeUiBuilder = mazeUiBuilder;
            _cameraManagement = cameraManagement;
            _inputHandler = inputHandler;
            _currentMazeHolder = currentMazeHolder;
            _generateTestCase = generateTestCase;
        }

        private int _currentLevel = 0;
        


        public void Awake()
        {
            //_generateTestCase.Run();

            _maze = _currentMazeHolder.MazeJumper;
            _cameraManagement.Init(CameraTransform, Camera, _maze);
            _inputHandler.Init(_cameraManagement, new InputHandlerOptions
            {
                MoveDown = () => {
                    if (_currentLevel > 0)
                    {
                        _currentLevel -= 1;
                        _mazeUiBuilder.BuildMazeUI(MazeField, _maze, _currentLevel);
                    }
                },
                MoveUp = () => {
                    if (_currentLevel < _maze.Size.Z - 1)
                    {
                        _currentLevel += 1;
                        _mazeUiBuilder.BuildMazeUI(MazeField, _maze, _currentLevel);
                    }
                },
                ToggleDeadEnds = () => {
                    _maze.ToggleDeadEnd();
                    _mazeUiBuilder.BuildMazeUI(MazeField, _maze, _currentLevel);
                }
            });

            _mazeUiBuilder.BuildMazeUI(MazeField, _maze, _currentLevel);
        }

        public void Update()
        {
            _inputHandler.HandleUpdate();
        }

    }
}
