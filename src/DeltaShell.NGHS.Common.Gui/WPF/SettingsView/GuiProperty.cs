using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;

namespace DeltaShell.NGHS.Common.Gui.WPF.SettingsView
{
    /// <summary>
    /// Class which is used to get the Gui Properties to use in the WPF view.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.ComponentModel.IDataErrorInfo" />
    public class GuiProperty : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly FieldUIDescription description;
        private Func<object> getModel;
        private ObservableCollection<DoubleWrapper> valueCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuiProperty"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        public GuiProperty(FieldUIDescription description)
        {
            CustomCommand = new CommandHelper(() => OnPropertyChanged("Value"));
            this.description = description;
            if (description?.CustomControlHelper != null)
            {
                var control = description.CustomControlHelper.CreateControl();
                var hostHelper = new WindowsFormsHost
                {
                    Child = control,
                    Width = 300,
                    Height = 300,
                    Background = new SolidColorBrush(SystemColors.ControlColor)
                };
                CustomControl = hostHelper;
            }

            UpdateValueCollection();
        }

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified column name.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                if (columnName != "Value" 
                    ||  string.IsNullOrEmpty(Value.ToString())) return null;

                if (description.HasMinValue || description.HasMaxValue)
                {
                    /*ToDo: investigate why properties with doubles do not validate with Error method.
                     For instance, MaxCourant, value = -5, minValue=0 does not return Validates in the Error getter.
                     If possible, remove this code and just make a 'return Error;'
                     */
                    if (Value is int)
                    {
                        return CheckValueWithinBoundaries(Convert.ToDouble((int)Value));
                    }

                    if (Value is double)
                    {
                        return CheckValueWithinBoundaries(Convert.ToDouble((double)Value));
                    }
                }

                return Error;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has custom control.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has custom control; otherwise, <c>false</c>.
        /// </value>
        public bool HasCustomControl
        {
            get { return CustomControl != null; }
        }

        /// <summary>
        /// Gets or sets the custom control.
        /// ToDo: This should be removed once all custom controls are migrated into WPF.
        /// </summary>
        /// <value>
        /// The custom control.
        /// </value>
        public FrameworkElement CustomControl { get; set; }

        public CommandHelper CustomCommand { get; set; }

        public Type ValueType { get { return description?.ValueType; } }

        public string Name { get { return description?.Name; } }

        public string SubCategory { get { return description?.SubCategory; } }
        
        public string Label { get { return description?.Label; } }

        public string ToolTip
        {
            get { return description?.ToolTip; }
        }

        public string UnitSymbol { get { return description?.UnitSymbol; } }

        /// <summary>
        /// Gets or sets the get model function that will be used later on for 
        /// retrieving the IsEnabled <seealso cref="IsEnabled"/>and IsVisible <seealso cref="IsVisible"/> properties.
        /// </summary>
        /// <value>
        /// The get model.
        /// </value>
        public Func<object> GetModel
        {
            get { return getModel; }
            set
            {
                getModel = value;
                CustomCommand.GetModel = getModel;
                if (getModel != null && HasCustomControl)
                {
                    UpdateCustomControlData();
                }
            }
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public string Error {
            get
            {
                var mssg = string.Empty;
                if (!description.Validate(GetModel.Invoke(), Value, out mssg))
                    return mssg;
                return null;
            }
        }

        public bool IsEditable
        {
            get { return IsEnabled && !CustomCommand.ButtonIsVisible; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get
            {
                if (description == null) return false;
                var model = GetModel?.Invoke();
                return description.IsEnabled(model);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get
            {
                if (description == null) return false;
                var model = GetModel?.Invoke();
                return description.IsVisible(model);
            }
        }

        /// <summary>
        /// Gets or sets the value collection (When applicable, otherwise it's just an empty list).
        /// </summary>
        /// <value>
        /// The value collection.
        /// </value>
        public ObservableCollection<DoubleWrapper> ValueCollection
        {
            get { return valueCollection; }
            set
            {
                valueCollection = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value
        {
            get { return description?.GetValue(GetModel?.Invoke()); }
            set
            {
                var convertedValue = value;
                if (value is string)
                {
                    convertedValue = Convert.ChangeType(value, ValueType);
                }

                description?.SetValue(GetModel?.Invoke(), convertedValue);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Raises the property changed events.
        /// </summary>
        public void RaisePropertyChangedEvents()
        {
            OnPropertyChanged("IsEnabled");
            OnPropertyChanged("IsVisible");
            OnPropertyChanged("IsEditable");
            OnPropertyChanged("Value");
            UpdateValueCollection();
            OnPropertyChanged("ValueCollection");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdateValueCollection()
        {
            var valueList = new List<DoubleWrapper>();
            if (ValueType == typeof(IList<double>) && Value is IList<double>)
            {
                var collection = Value as IList<double>;
                valueList = collection.Select(
                    v => new DoubleWrapper
                    {
                        WrapperValue = v,
                        SetBackValue = wrapperValue =>
                        {
                            v = wrapperValue; //Overwrite value with new one.
                            Value = ValueCollection.Select(vc => vc.WrapperValue).ToList(); //Trigger update.
                        },
                    }).ToList();
            }
            ValueCollection = new ObservableCollection<DoubleWrapper>(valueList); //Just to avoid a null exception
        }

        private string CheckValueWithinBoundaries(double valueAsDouble)
        {
            if ((description.HasMinValue && valueAsDouble < description.MinValue) 
                || (description.HasMaxValue && valueAsDouble > description.MaxValue))
            {
                {
                    return string.Format(NGHS.Common.Gui.Properties.Resources.WpfGuiProperty_CheckValueWithinBoundaries_This_value_must_be_between__0__and__1_,
                        description.HasMinValue ? description.MinValue : double.NegativeInfinity,
                        description.HasMaxValue ? description.MaxValue : double.PositiveInfinity);
                }
            }

            return null;
        }

        private void UpdateCustomControlData()
        {
            if (description?.CustomControlHelper != null)
            {
                var control = CustomControl as WindowsFormsHost;
                var hostedControl = control?.Child;
                description.CustomControlHelper.SetData(hostedControl, getModel?.Invoke(), null);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}