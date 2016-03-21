using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI.Menu.Settings;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public interface IBaseValidateSettings
    {
        IEnumerable<ValidationResult> ValidateSetting(MazeGenerationSettings settings);
    }
}
