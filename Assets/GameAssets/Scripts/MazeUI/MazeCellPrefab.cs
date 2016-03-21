using UnityEngine;
using Zenject;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class MazeCellPrefab : MonoBehaviour {

        public SpriteRenderer background;
        public SpriteRenderer up;
        public SpriteRenderer down;

        // Use this for initialization
        void Start () {
	
        }

        void Awake()
        {
            
        }

        [PostInject]
        public void Init()
        {

        }

        // Update is called once per frame
        void Update () {
	        
        }
    }
}
