using System.Globalization;
using System.Windows.Controls;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Validation
{
    /// <summary>
    /// Validation rule for validating whether the input value is a positive double.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.ValidationRule"/>
    public class PositiveDoubleValidationRule : ValidationRule
    {
        /// <summary>
        /// Validates the specified <paramref name="value"/> to be a positive double value.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <param name="cultureInfo"> The culture info. </param>
        /// <remarks> <paramref name="value"/> is expected to be a <see cref="string"/>. </remarks>
        /// <returns>
        /// A valid <see cref="ValidationResult"/> when the <paramref name="value"/> is a positive double value;
        /// otherwise an invalid <see cref="ValidationResult"/>.
        /// </returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool result = double.TryParse((string) value, NumberStyles.Any, cultureInfo, out double doubleValue);
            if (result && doubleValue >= 0 && !double.IsNaN(doubleValue) && !double.IsInfinity(doubleValue))
            {
                return new ValidationResult(true, null);
            }

            return new ValidationResult(false, null);
        }
    }
}