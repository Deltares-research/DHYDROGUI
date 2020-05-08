using System;
using System.Globalization;
using System.Windows.Controls;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    /// <summary>
    /// This class is used for validating data from the WeirViewWpf.xaml.
    /// It only validates the text boxes where this ValidationRule is bound to the Textbox "Text" property.
    /// </summary>
    /// <inheritdoc cref="ValidationRule"/>
    public class CrestValidationRule : ValidationRule
    {
        /// <summary>Performs validation checks on a value.</summary>
        /// <param name="value">The value from the binding target to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>A <see cref="T:System.Windows.Controls.ValidationResult"/> object.</returns>
        /// <exception cref="FormatException">Thrown when the user enters something in a wrong format f.e. string</exception>
        /// <exception cref="InvalidCastException">Thrown when the entered value cannot be converted to a double</exception>
        /// <exception cref="OverflowException">Thrown when the number is either too large or too small</exception>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double result;
            try
            {
                result = Convert.ToDouble(value);
            }

            catch (FormatException)
            {
                return new ValidationResult(false,
                                            Resources.CrestValidationRule_Validate_The_entered_value_is_not_a_number__please_enter_a_number_greater_than_0);
            }
            catch (InvalidCastException)
            {
                return new ValidationResult(false,
                                            Resources.CrestValidationRule_Validate_The_entered_value_is_not_a_number__please_enter_a_number_greater_than_0);
            }
            catch (OverflowException)
            {
                return new ValidationResult(false,
                                            Resources.CrestValidationRule_Validate_The_entered_value_is_either_too_large_or_too_small__please_enter_a_number_between__1_79769313486232E_308___1_79769313486232E_308);
            }

            return result > 0.0
                       ? ValidationResult.ValidResult
                       : new ValidationResult(false, Resources.CrestValidationRule_Validate_The_entered_value_must_be_greater_than_0);
        }
    }
}