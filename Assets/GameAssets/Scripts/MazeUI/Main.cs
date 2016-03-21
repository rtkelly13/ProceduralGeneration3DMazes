using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;
using Zenject;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class Main : MonoBehaviour
    {
        private IMazeGenerationFactory _generationFactory;

        [PostInject]
        public void Init(IMazeGenerationFactory generationFactory)
        {
            _generationFactory = generationFactory;
        }


    }
}
