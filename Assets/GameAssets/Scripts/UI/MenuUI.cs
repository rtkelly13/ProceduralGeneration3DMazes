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

        public 
            // Use this for initialization
            void Start () {
	    
            }

        void Awake()
        {
            var list = new List<DropdownOption<String, System.Object>>();
            InstantiateControl<DropdownControl>(leftPanel).Initialise("Algorithms", list, key =>
            {
                var algorithmName = key;
            });

           
            InstantiateControl<SliderControl>(leftPanel).Initialize("Cell size", 1, 10, 0, value =>
            {
                var test = value;
            });
        }
	
        // Update is called once per frame
        void Update () {
	
        }
    }
} 