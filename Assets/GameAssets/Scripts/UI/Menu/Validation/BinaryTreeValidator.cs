using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public class BinaryTreeValidator : IValidateSetting
    {
        private readonly IBaseValidateSettings _baseValidateSettings;
        public Algorithm AlgorithmType { get; private set; }

        public BinaryTreeValidator(IBaseValidateSettings baseValidateSettings)
        {
            _baseValidateSettings = baseValidateSettings;
            AlgorithmType = Algorithm.BinaryTreeAlgorithm;
        }

        public IEnumerable<ValidationResult> ValidateSetting(MazeGenerationSettings settings)
        {
            return _baseValidateSettings.ValidateSetting(settings);
        }
    }
}
