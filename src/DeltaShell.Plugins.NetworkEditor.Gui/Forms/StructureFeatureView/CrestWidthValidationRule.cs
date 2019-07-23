using System.Globalization;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    /// <summary>
    /// This class is responsible for validating Crest Width fields.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.CrestValidationRule" />
    /// <inheritdoc cref="CrestValidationRule"/>
    public class CrestWidthValidationRule : CrestValidationRule
    {
        /// <summary>
        /// Perform a validation check on the specified value.
        /// </summary>
        /// <param name="value">The value from the binding target to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Controls.ValidationResult" /> object.
        /// </returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // Check if the value is empty, which corresponds with the default value
            if (value is string s && string.IsNullOrEmpty(s))
                return ValidationResult.ValidResult;

            return base.Validate(value, cultureInfo);
        }
    }
}
