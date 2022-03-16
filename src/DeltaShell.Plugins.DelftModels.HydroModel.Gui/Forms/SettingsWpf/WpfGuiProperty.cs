using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Class which is used to get the Gui Properties to use in the WPF view.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged"/>
    /// <seealso cref="System.ComponentModel.IDataErrorInfo"/>
    public sealed class WpfGuiProperty : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly FieldUIDescription description;
        private Func<object> getModel;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfGuiProperty"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        public WpfGuiProperty(FieldUIDescription description)
        {
            CustomCommand = new CommandHelper(() => OnPropertyChanged("Value"));
            this.description = description;

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
                    || string.IsNullOrEmpty(Value.ToString()))
                {
                    return null;
                }

                if (description.HasMinValue || description.HasMaxValue)
                {
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

        public CommandHelper CustomCommand { get; set; }

        public Type ValueType => description?.ValueType;

        public string Name => description?.Name;

        public string SubCategory => description?.SubCategory;

        public string Label => description?.Label;

        public string ToolTip => description?.ToolTip ?? "-";

        /// <summary>
        /// Unit for this property
        /// </summary>
        public string UnitSymbol
        {
            get
            {
                string unitSymbol = description?.UnitSymbol;
                unitSymbol = GetEnumerableSymbol(unitSymbol);
                return !string.IsNullOrEmpty(unitSymbol)
                           ? $"[{unitSymbol}]"
                           : "";
            }
        }

        public bool IsEnumerableSymbol => description != null && (description.UnitSymbol?.Contains("|") ?? false);

        public Func<string, WpfGuiProperty> GetBindedProperty { get; set; }

        /// <summary>
        /// Minimum allowed value
        /// </summary>
        public double? MinValue => description?.MinValue;

        /// <summary>
        /// Maximum allowed value
        /// </summary>
        public double? MaxValue => description?.MaxValue;

        /// <summary>
        /// Has a minimum value is set
        /// </summary>
        public bool? HasMinValue => description?.HasMinValue;

        /// <summary>
        /// Has a maximum value is set
        /// </summary>
        public bool? HasMaxValue => description?.HasMaxValue;

        /// <summary>
        /// Has a minimum or a maximum value set
        /// </summary>
        public bool? HasMinMaxValue =>
            HasMaxValue.HasValue &&
            HasMaxValue.Value ||
            HasMinValue.HasValue &&
            HasMinValue.Value;

        /// <summary>
        /// Gets or sets the get model function that will be used later on for
        /// retrieving the IsEnabled <seealso cref="IsEnabled"/>and IsVisible <seealso cref="IsVisible"/> properties.
        /// </summary>
        /// <value>
        /// The get model.
        /// </value>
        public Func<object> GetModel
        {
            get => getModel;
            set
            {
                getModel = value;
                CustomCommand.GetModel = getModel;
            }
        }

        /// <summary>
        /// Gets whether the property is read only.
        /// </summary>
        public bool IsReadOnly => !IsEnabled || !CustomCommand.TextBoxEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get
            {
                if (description == null)
                {
                    return false;
                }

                object model = GetModel?.Invoke();
                return description.IsEnabled(model);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get
            {
                if (description == null)
                {
                    return false;
                }

                object model = GetModel?.Invoke();
                return description.IsVisible(model);
            }
        }

        /// <summary>
        /// Gets or sets the value collection (When applicable, otherwise it's just an empty list).
        /// </summary>
        /// <value>
        /// The value collection.
        /// </value>
        public ObservableCollection<DoubleWrapper> ValueCollection { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value
        {
            get => description?.GetValue(GetModel?.Invoke());
            set
            {
                object convertedValue = value;

                if (ValueType.Implements(typeof(IConvertible)))
                {
                    convertedValue = Convert.ChangeType(value, ValueType);
                }

                description?.SetValue(GetModel?.Invoke(), convertedValue);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public string Error
        {
            get
            {
                var errorMessage = string.Empty;
                return !description.Validate(GetModel.Invoke(), Value, out errorMessage) ? errorMessage : null;
            }
        }

        /// <summary>
        /// Raises the property changed events.
        /// </summary>
        public void RaisePropertyChangedEvents()
        {
            OnPropertyChanged(nameof(IsEnabled));
            OnPropertyChanged(nameof(IsVisible));
            OnPropertyChanged(nameof(IsReadOnly));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(UnitSymbol));
            UpdateValueCollection();
            OnPropertyChanged(nameof(ValueCollection));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetEnumerableSymbol(string unformattedSymbol)
        {
            if (unformattedSymbol == null || !IsEnumerableSymbol || GetBindedProperty == null)
            {
                return unformattedSymbol;
            }

            const char symbolSeparator = ':';
            string bindedProperty = unformattedSymbol.Split(symbolSeparator)[0];
            string[] symbols = unformattedSymbol.Split(symbolSeparator)[1].Split('|');
            WpfGuiProperty lbv = GetBindedProperty.Invoke(bindedProperty);
            var enumVal = (int)Enum.Parse(lbv.ValueType, lbv.Value.ToString());
            if (symbols.Length <= enumVal)
            {
                return symbols[0].Trim();
            }

            return symbols[enumVal].Trim();
        }

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
                            v = wrapperValue;                                               //Overwrite value with new one.
                            Value = ValueCollection.Select(vc => vc.WrapperValue).ToList(); //Trigger update.
                        }
                    }).ToList();
            }

            ValueCollection = new ObservableCollection<DoubleWrapper>(valueList); //Just to avoid a null exception
        }

        private string CheckValueWithinBoundaries(double valueAsDouble)
        {
            if (description.HasMinValue && valueAsDouble < description.MinValue
                || description.HasMaxValue && valueAsDouble > description.MaxValue)
            {
                return string.Format(Resources.WpfGuiProperty_this_This_value_must_be_between__0__and__1_,
                                     description.HasMinValue ? description.MinValue : double.NegativeInfinity,
                                     description.HasMaxValue ? description.MaxValue : double.PositiveInfinity);
            }

            return null;
        }
    }
}