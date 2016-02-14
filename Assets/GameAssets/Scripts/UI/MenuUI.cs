using System;
using System.Collections.Generic;
using Assets.Examples.UI;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI
{
    public class MenuUI : UIBase
    {
        public RectTransform leftPanel;
        public RectTransform rightPanel;
        private IAlgorithmsProvider _algorithmProvider;

        public MenuUI()
        {
            var container = DependencyContainer.Container;
            _algorithmProvider = container.Resolve<IAlgorithmsProvider>();
        }
        // Use this for initialization
        void Start ()
        {
            

        }

        void Awake()
        {
            var list = _algorithmProvider.DropdownOptions;
            InstantiateControl<DropdownControl>(leftPanel).Initialise("Algorithms", list, 0, key =>
            {
                var algorithmName = key;

            });

            rightPanel.

        }
	
        // Update is called once per frame
        void Update () {
	
        }
    }
} 