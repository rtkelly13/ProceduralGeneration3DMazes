using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public class ValidationResult
    {
        public ValidationResult(string field, bool isValid, string reason)
        {
            Field = field;
            IsValid = isValid;
            Reason = reason;
        }

        public ValidationResult()
        {
            
        }

        public string Field { get; set; }
        public bool IsValid { get; set; }
        public string Reason { get; set; }
    }
}
