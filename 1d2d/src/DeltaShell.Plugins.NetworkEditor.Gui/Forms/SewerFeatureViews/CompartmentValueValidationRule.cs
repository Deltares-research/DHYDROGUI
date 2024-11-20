using System;
using System.Globalization;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class CompartmentValueValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Null value is invalid");
            }

            double doubleValue;
            try
            {
                doubleValue = Convert.ToDouble(value, cultureInfo);
            }
            catch
            {
                return new ValidationResult(false, "The value of this parameter must be a double precision number.");
            }

            if (doubleValue <= 0)
            {
                return new ValidationResult(false, "The value of this parameter must be larger than 0.");
            }

            return new ValidationResult(true, null);
        }
    }
}