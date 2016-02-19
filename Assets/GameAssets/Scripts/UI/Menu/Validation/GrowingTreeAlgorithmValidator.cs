using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.UI.Menu.Settings;
using Assets.GameAssets.Scripts.UI.Menu.Settings.GrowingTree;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public class GrowingTreeAlgorithmValidator: IValidateSetting
    {
        private readonly IBaseValidateSettings _baseValidateSettings;
        public Algorithm AlgorithmType { get; private set; }

        public GrowingTreeAlgorithmValidator(IBaseValidateSettings baseValidateSettings)
        {
            _baseValidateSettings = baseValidateSettings;
            AlgorithmType = Algorithm.GrowingTreeAlgorithm;
        }

        public IEnumerable<ValidationResult> ValidateSetting(AlgorithmSettings settings)
        {
            var results = _baseValidateSettings.ValidateSetting(settings);
            foreach (var result in results)
            {
                yield return result;
            }
            var growingTreeSettings = settings as GrowingTreeSettings;
            if (growingTreeSettings == null)
            {
                throw new ArgumentException("Settings must not be null");
            }
            if (!growingTreeSettings.Strategies.Any())
            {
                yield return new ValidationResult(null, false, "There must be a weighted strategy with a value greater than 0");
            }
        } 
    }
}
