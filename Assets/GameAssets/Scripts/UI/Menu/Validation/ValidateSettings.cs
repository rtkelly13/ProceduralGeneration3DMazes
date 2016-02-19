using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.UI.Menu.Settings;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public class ValidateSettings : IValidateSettings
    {
        private readonly List<IValidateSetting> _validators;
        private readonly IValidationRetriever _validationRetriever;

        public ValidateSettings(List<IValidateSetting> validators, IValidationRetriever validationRetriever)
        {
            _validators = validators;
            _validationRetriever = validationRetriever;
        }

        public IEnumerable<ValidationResult> Validate(AlgorithmSettings settings)
        {
            var validatorStrategy = _validators.SingleOrDefault(x => x.AlgorithmType == settings.Algorithm);
            if (validatorStrategy != null)
            {
                return validatorStrategy.ValidateSetting(settings);
            }
            else
            {
                throw new ArgumentException("Unsupported Algorithm Type");
            }
        }

        public ValidationResult ValidateAndProcess(AlgorithmSettings settings)
        {
            return _validationRetriever.GetOverallValidationResult(Validate(settings));
        }
    }
}
