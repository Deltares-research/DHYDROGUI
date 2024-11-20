using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class SedimentPropertyValidationRule : ValidationRule
    {
        public ComparisonValue MinValue { get; set; }
        public ComparisonValue MaxValue { get; set; }

        public ComparisonBoolValue MinIsOpened { get; set; }
        public ComparisonBoolValue MaxIsOpened { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(false, "Null value is invalid");

			double doubleValue;
			try
            {
                doubleValue = Convert.ToDouble(value, cultureInfo);
            }
            catch
            {
                return new ValidationResult(false, "The value of this parameter must be a double precision number.");
            }

			var minOp = MinIsOpened != null ? MinIsOpened.Value : false;
	        var maxOp = MaxIsOpened != null ? MaxIsOpened.Value : false;

			if (minOp && maxOp)
            {
                return (doubleValue > MinValue.Value && doubleValue < MaxValue.Value)
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, string.Format("The value of this parameter must be in the interval ({0},{1})", MinValue, MaxValue));
            }

            if (minOp)
            {
                return (doubleValue > MinValue.Value && doubleValue <= MaxValue.Value)
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, string.Format("The value of this parameter must be in the interval ({0},{1}]", MinValue, MaxValue));
            }

            if (maxOp)
            {
                return (doubleValue >= MinValue.Value && doubleValue < MaxValue.Value)
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, string.Format("The value of this parameter must be in the interval [{0},{1})", MinValue, MaxValue));
            }

            return (doubleValue >= MinValue.Value && doubleValue <= MaxValue.Value)
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, string.Format("The value of this parameter must be in the interval [{0},{1}]", MinValue, MaxValue));
        }
    }

    public class ComparisonValue : DependencyObject
    {
        private static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
            typeof(double), typeof(ComparisonValue));
        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }

	public class ComparisonBoolValue : DependencyObject
    {
        private static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
            typeof(bool), typeof(ComparisonBoolValue));
        public bool Value
        {
            get { return (bool)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
		public override string ToString()
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}
	}

    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
        
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}