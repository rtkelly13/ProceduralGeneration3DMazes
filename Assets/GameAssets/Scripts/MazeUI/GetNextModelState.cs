using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class ModelStateHelper : IModelStateHelper
    {
        public void SetNextModelState(IModelState modelState)
        {
            switch (modelState.ModelMode)
            {
                case ModelMode.Standard:
                    modelState.SetState(ModelMode.DeadEndFilled);
                    break;
                case ModelMode.DeadEndFilled:
                    modelState.SetState(ModelMode.Standard);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
