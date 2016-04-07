using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public class RecursiveBacktrackerValidator : IValidateSetting
    {
        private readonly IBaseValidateSettings _baseValidateSettings;
        public Algorithm AlgorithmType { get; private set; }

        public RecursiveBacktrackerValidator(IBaseValidateSettings baseValidateSettings)
        {
            _baseValidateSettings = baseValidateSettings;
            AlgorithmType = Algorithm.RecursiveBacktrackerAlgorithm;
        }

        public IEnumerable<ValidationResult> ValidateSetting(MazeGenerationSettings settings)
        {
            return _baseValidateSettings.ValidateSetting(settings);
        }
    }
}
