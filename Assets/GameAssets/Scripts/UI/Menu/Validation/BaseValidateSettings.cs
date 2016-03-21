using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI.Menu.Settings;
using ModestTree;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public class BaseValidateSettings : IBaseValidateSettings
    {
        public IEnumerable<ValidationResult> ValidateSetting(MazeGenerationSettings settings)
        {
            if (settings.Algorithm == Algorithm.None)
            {
                yield return new ValidationResult(null, false, "An Algorithm Has to be selected for the Maze Generation tool to run.");
            }
            if (settings.Size.X < 1)
            {
                yield return new ValidationResult(null, false, "The X value must be greater than 0");
            }
            if (settings.Size.X > 100)
            {
                yield return new ValidationResult(null, false, "The X value must be less than 100");
            }
            if (settings.Size.Y < 1)
            {
                yield return new ValidationResult(null, false, "The Y value must be greater than 0");
            }
            if (settings.Size.Y > 100)
            {
                yield return new ValidationResult(null, false, "The Y value must be less than 100");
            }
            if (settings.Size.Z < 1)
            {
                yield return new ValidationResult(null, false, "The Z value must be greater than 0");
            }
            if (settings.Size.Z > 100)
            {
                yield return new ValidationResult(null, false, "The Z value must be less than 100");
            }
            if (settings.Option == MazeType.None)
            {
                yield return new ValidationResult(null, false, "A Model Option must be selected");
            }
            

        }
    }
}