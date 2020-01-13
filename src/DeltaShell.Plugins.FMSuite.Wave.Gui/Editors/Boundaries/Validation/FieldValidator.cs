using System.Globalization;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Validation
{
    /// <summary>
    /// Validator for field inputs
    /// </summary>
    public static class FieldValidator
    {
        /// <summary>
        /// Validates whether the specified <paramref name="value" /> is a positive double.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <param name="cultureInfo"> The culture info. </param>
        /// <returns>
        /// <c> true </c> when <paramref name="value" /> is a positive double value;
        /// otherwise, <c> false </c>.
        /// </returns>
        public static bool IsPositiveDouble(string value, CultureInfo cultureInfo)
        {
            bool result = double.TryParse(value, NumberStyles.Any, cultureInfo, out double doubleValue);
            if (result == false || doubleValue < 0 || double.IsNaN(doubleValue))
            {
                return false;
            }

            return true;
        }
    }
}