using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI.Menu.Settings;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public interface IValidateSettings
    {
        IEnumerable<ValidationResult> Validate(MazeGenerationSettings settings);
        ValidationResult ValidateAndProcess(MazeGenerationSettings settings);
    }
}