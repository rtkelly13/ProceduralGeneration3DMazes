using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public class SceneLoader : ISceneLoader
    {
        public void LoadMaze()
        {
            LoadScene("Maze");
        }

        public void LoadMazeLoader()
        {
            LoadScene("MazeLoader");
        }

        public void LoadMenu()
        {
            LoadScene("Menu");
        }

        private void LoadScene(string name)
        {
            SceneManager.LoadScene(name);
        }
    }
}
