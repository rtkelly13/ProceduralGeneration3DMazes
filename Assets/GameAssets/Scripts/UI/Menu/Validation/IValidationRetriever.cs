using System.Collections.Generic;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public interface IValidationRetriever
    {
        ValidationResult GetOverallValidationResult(IEnumerable<ValidationResult> results);
    }
}