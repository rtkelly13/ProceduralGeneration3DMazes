using System;
using System.Collections.Generic;
using Assets.Examples.UI;
using Assets.GameAssets.Scripts.Maze;
using Assets.GameAssets.Scripts.UI.Controls;
using Assets.GameAssets.Scripts.UI.Helper;
using UnityEngine;
using Zenject;

namespace Assets.GameAssets.Scripts.UI
{
    public class MenuUI : MonoBehaviour
    {
        private IAlgorithmsProvider _algorithmsProvider;
        private IResourceLoader _resourceLoader;
        public RectTransform leftPanel;
        public RectTransform rightPanel;

        public MenuUI()
        {
           
        }


        [PostInject]
        public void Init(IAlgorithmsProvider algorithmsProvider, IResourceLoader resourceLoader)
        {
            _algorithmsProvider = algorithmsProvider;
            _resourceLoader = resourceLoader;
        }
        // Use this for initialization
        void Start ()
        {

        }

        void Awake()
        {
            var list = _algorithmsProvider.DropdownOptions;
            _resourceLoader.InstantiateControl<DropdownControl>(leftPanel).Initialise("Algorithms", list, 0, key =>
            {
                var algorithmName = key;
                rightPanel.Clear();
            });

            _resourceLoader.InstantiateControl<TextControl>(rightPanel).Initialize("Test Text");
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