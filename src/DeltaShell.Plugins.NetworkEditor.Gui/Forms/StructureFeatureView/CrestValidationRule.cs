using System;
using System.Globalization;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    /// <summary>
    /// This class is used for validating data from the WeirViewWpf.xaml.
    /// It only validates the text boxes where this ValidationRule is bound to the Textbox "Text" property.
    /// </summary>
    public class CrestValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double result;
            try
            {
                result = Convert.ToDouble(value);
            }

            catch (FormatException)
            {
                return new ValidationResult(false, "The entered value is not a number, please enter a number greater than 0");
            }
            catch (InvalidCastException)
            {
                return new ValidationResult(false, "The entered value is not a number, please enter a number greater than 0");
            }
            catch (OverflowException)
            {
                return new ValidationResult(false, "The entered value is either too large or too small, please enter a number between -1.79769313486232E+308 & 1.79769313486232E+308");
            }

            return result > 0.0 ? ValidationResult.ValidResult : new ValidationResult(false, "The entered value must be greater than 0");
        }
    }
}

