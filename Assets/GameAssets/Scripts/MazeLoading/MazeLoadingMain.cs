using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public RectTransform topPanel;
        public Text validationResult;
        public Button button;

        private ICurrentSettingsHolder _currentSettingsHolder;
        private IMazeGenerationFactory _generationFactory;
        private IMazeValidator _validator;
        private ICurrentMazeHolder _currentMazeHolder;
        private ISceneLoader _sceneLoader;

        [PostInject]
        public void Init(IMazeGenerationFactory generationFactory, ICurrentSettingsHolder currentSettingsHolder, IMazeValidator validator, ICurrentMazeHolder currentMazeHolder, ISceneLoader sceneLoader)
        {
            _generationFactory = generationFactory;
            _currentSettingsHolder = currentSettingsHolder;
            _validator = validator;
            _currentMazeHolder = currentMazeHolder;
            _sceneLoader = sceneLoader;
        }

        public void Awake()
        {
            button.enabled = false;
            button.onClick.AddListener(OnClick);

            validationResult.text = "";
            _currentMazeHolder.MazeJumper = _generationFactory.GenerateMaze(_currentSettingsHolder.Settings);
            var validation = _validator.EveryPointHasDirection(_currentMazeHolder.MazeJumper);
            button.enabled = true;
        }

        private void OnClick()
        {
            _sceneLoader.LoadMaze();
        }


    }
}
