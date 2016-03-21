using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI.Menu.Settings;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public class NoneAlgorithmValidator: IValidateSetting
    {
        private readonly IBaseValidateSettings _baseValidateSettings;
        public Algorithm AlgorithmType { get; private set; }

        public NoneAlgorithmValidator(IBaseValidateSettings baseValidateSettings)
        {
            _baseValidateSettings = baseValidateSettings;
            AlgorithmType = Algorithm.None;
        }

        public IEnumerable<ValidationResult> ValidateSetting(MazeGenerationSettings settings)
        {
            var results = _baseValidateSettings.ValidateSetting(settings);
            foreach (var result in results)
            {
                yield return result;
            }
        }

    }
}
