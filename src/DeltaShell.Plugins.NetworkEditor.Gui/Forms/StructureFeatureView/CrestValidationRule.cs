using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

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
            var extractedValue = ExtractBoundValue(value);
            double result;
            try
            {
                result = Convert.ToDouble(extractedValue);
            }

            catch (FormatException)
            {
                return new ValidationResult(false,
                    "The entered value is not a number, please enter a number greater than 0");
            }
            catch (InvalidCastException)
            {
                return new ValidationResult(false,
                    "The entered value is not a number, please enter a number greater than 0");
            }
            catch (OverflowException)
            {
                return new ValidationResult(false,
                    "The entered value is either too large or too small, please enter a number between -1.79769313486232E+308 & 1.79769313486232E+308");
            }

            return result > 0.0
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "The entered value must be greater than 0");
        }

        /// <summary>
        /// In some cases the validation needs to occur AFTER setting the value(by default the value will be validated before setting the property).
        /// If this is necessary we have set ValidationStep="UpdatedValue" in the XAML.
        /// 'value' will then be of type 'BindingExpression' instead of the value itself.
        /// To extract the value, we need to execute this method. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>"the extracted value as an object"</returns>
        private static object ExtractBoundValue(object value)
        {
            var binding = value as BindingExpression;
            if (binding == null) return value;

            var dataItem = binding.DataItem;
            var propertyName = binding.ParentBinding.Path.Path;

            return dataItem.GetType().GetProperty(propertyName)?.GetValue(dataItem, null);
        }
    }
}

