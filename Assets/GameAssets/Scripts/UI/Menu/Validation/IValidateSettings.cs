using System.Collections.Generic;
using Assets.GameAssets.Scripts.UI.Menu.Settings;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public interface IValidateSettings
    {
        IEnumerable<ValidationResult> Validate(AlgorithmSettings settings);
        ValidationResult ValidateAndProcess(AlgorithmSettings settings);
    }
}