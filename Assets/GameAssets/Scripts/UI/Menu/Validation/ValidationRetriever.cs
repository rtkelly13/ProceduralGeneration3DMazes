using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public class ValidationRetriever : IValidationRetriever
    {
        public ValidationResult GetOverallValidationResult(IEnumerable<ValidationResult> results)
        {
            var first = results.FirstOrDefault(x => !x.IsValid);
            return first ?? new ValidationResult(null, true, "");
        }
    }
}
