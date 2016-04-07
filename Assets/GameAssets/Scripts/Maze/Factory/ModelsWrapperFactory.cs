using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public class ModelsWrapperFactory : IModelsWrapperFactory
    {
        public IModelsWrapper Make(IModelBuilder modelBuilder)
        {
            return new ModelsWrapper(modelBuilder);
        }
    }
}
