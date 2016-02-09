using System;
using Assets.Examples.UI;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI
{
    public class GameSetup : UIBase {

        public Camera MainCam { get; set; }
        // Use this for initialization
        void Start () {
	        Console.WriteLine("Test");
        }

        private void Awake()
        {
            //var header = InstantiateControl<TextControl>(algorithmsGroup.transform.parent);
            //header.Initialize("Generator algorithm");
            //header.transform.SetAsFirstSibling();
        }
	
        // Update is called once per frame
        void Update () {
	
        }
    }
}
