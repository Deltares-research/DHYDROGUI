using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Aop;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    [Entity]
    public abstract class ModelProperty : ICloneable
    {
        private readonly ModelPropertyDefinition propertyDefinition;
        private object value;

        /// <summary>
        /// Create a new model property.
        /// </summary>
        /// <param name="propertyDefinition"> Property definition for this property. </param>
        /// <param name="valueAsString"> String representing the initial value of this property. </param>
        /// <exception cref="ArgumentNullException"> When <paramref name="propertyDefinition"/> is null. </exception>
        /// <exception cref="FormatException">
        /// When <paramref name="valueAsString"/> does not properly express the <see cref="ModelPropertyDefinition.DataType"/>
        /// specified in <see cref="PropertyDefinition"/>. Check <see cref="System.Exception.InnerException"/> for underlying
        /// cause.
        /// </exception>
        protected ModelProperty(ModelPropertyDefinition propertyDefinition, string valueAsString)
        {
            this.propertyDefinition = propertyDefinition;

            if (propertyDefinition == null)
            {
                throw new ArgumentNullException(nameof(propertyDefinition));
            }

            string inputString = string.IsNullOrEmpty(valueAsString)
                                     ? propertyDefinition.DefaultValueAsString
                                     : valueAsString;
            if (propertyDefinition.DataType == typeof(Steerable))
            {
                value = new Steerable();
            }

            SetValueAsString(inputString);
        }

        /// <summary>
        /// The value of the property.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When type of value does not conform to
        /// <see cref="ModelPropertyDefinition.DataType"/> specified in <see cref="PropertyDefinition"/>.
        /// </exception>
        public object Value
        {
            get => value;
            set
            {
                if (value != null)
                {
                    ThrowIfTypesDoNotMatch(value.GetType());
                }

                this.value = value;
            }
        }

        /// <summary>
        /// The minimum allowed value if defined (object type defined by <see cref="ModelPropertyDefinition.DataType"/>)
        /// or null if not set.
        /// </summary>
        public object MinValue => !string.IsNullOrEmpty(propertyDefinition.MinValueAsString)
                                      ? FMParser.FromString(propertyDefinition.MinValueAsString, propertyDefinition.DataType)
                                      : null;

        /// <summary>
        /// The maximum allowed value if defined (object type defined by <see cref="ModelPropertyDefinition.DataType"/>)
        /// or null if not set.
        /// </summary>
        public object MaxValue => !string.IsNullOrEmpty(propertyDefinition.MaxValueAsString)
                                      ? FMParser.FromString(propertyDefinition.MaxValueAsString, propertyDefinition.DataType)
                                      : null;

        /// <summary>
        /// The description and definition of this property.
        /// </summary>
        public ModelPropertyDefinition PropertyDefinition => propertyDefinition;

        /// <summary>
        /// Sets <see cref="Value"/> using a string representation.
        /// </summary>
        /// <param name="valueAsString"> String representation of the new <see cref="Value"/>. </param>
        /// <exception cref="FormatException">
        /// When <paramref name="valueAsString"/> does not properly express the <see cref="ModelPropertyDefinition.DataType"/>
        /// specified in <see cref="PropertyDefinition"/>. Check <see cref="System.Exception.InnerException"/> for underlying
        /// cause.
        /// </exception>
        public void SetValueAsString(string valueAsString)
        {
            try
            {
                if (propertyDefinition.DataType == typeof(Steerable))
                {
                    SetValueAsStringForSteerable(valueAsString);
                }
                else
                {
                    Value = FMParser.FromString(valueAsString, propertyDefinition.DataType);
                }
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is FormatException)
                {
                    throw new FormatException($"Unexpected value string \"{valueAsString}\" for property \"{PropertyDefinition.FilePropertyKey}\"",
                                              e);
                }

                if (e is OverflowException)
                {
                    throw new FormatException($"Value string \"{valueAsString}\" is too large/small for property \"{PropertyDefinition.FilePropertyKey}\"",
                                              e);
                }

                // Unexpected exception type, let it continue
                throw;
            }
        }

        /// <summary>
        /// Returns <see cref="Value"/> in string representation.
        /// </summary>
        public virtual string GetValueAsString()
        {
            return FMParser.ToString(value, propertyDefinition.DataType);
        }

        /// <summary>
        /// Checks if <see cref="Value"/> is valid.
        /// </summary>
        /// <returns> True if valid; False otherwise. </returns>
        public bool Validate()
        {
            if (propertyDefinition.DataType == typeof(string) || propertyDefinition.DataType == typeof(bool))
            {
                return true;
            }

            if (MinValue is IComparable minValue && minValue.CompareTo(value) > 0)
            {
                return false;
            }

            if (MaxValue is IComparable maxValue && maxValue.CompareTo(value) < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Indicates if this property is enabled or not.
        /// </summary>
        /// <param name="properties"> All available properties indexed on <see cref="ModelPropertyDefinition.FilePropertyKey"/>. </param>
        /// <returns> True if enabled; False otherwise. </returns>
        public bool IsEnabled(IEnumerable<ModelProperty> properties)
        {
            return propertyDefinition.IsEnabled(properties);
        }

        public bool IsVisible(IEnumerable<ModelProperty> properties)
        {
            return propertyDefinition.IsVisible(properties);
        }

        public override string ToString()
        {
            return $"{PropertyDefinition.Caption}: {GetValueAsString()} ({PropertyDefinition.Category})";
        }

        public abstract object Clone();

        /// <summary>
        /// Set value logic for when <see cref="Value"/> is of type <see cref="Steerable"/>.
        /// </summary>
        /// <param name="valueAsString"> </param>
        /// <exception cref="ArgumentException"> When <see cref="valueAsString"/> is an invalid file name. </exception>
        private void SetValueAsStringForSteerable(string valueAsString)
        {
            var steerableValue = (Steerable) value;

            if (valueAsString == "REALTIME") // It is then either driven externally...
            {
                steerableValue.Mode = SteerableMode.External;
                return;
            }

            string fileName = Path.GetFileName(valueAsString);
            if (FileUtils.IsValidFileName(fileName) && Path.GetExtension(fileName) == ".tim") // ... or a time series
            {
                steerableValue.TimeSeriesFilename = valueAsString;
                steerableValue.Mode = SteerableMode.TimeSeries;
                return;
            }

            var constantValue = FMParser.FromString<double>(valueAsString);
            steerableValue.ConstantValue = constantValue;
            steerableValue.Mode = SteerableMode.ConstantValue;
        }

        /// <summary>
        /// Precondition check to for checking if a value type corresponds to <see cref="ModelPropertyDefinition"/>.
        /// <see cref="ModelPropertyDefinition.DataType"/>.
        /// </summary>
        /// <param name="type"> Type to be checked. </param>
        /// <exception cref="ArgumentException"> When <paramref name="type"/> does not correspond. </exception>
        private void ThrowIfTypesDoNotMatch(Type type)
        {
            if (type != propertyDefinition.DataType && !type.Implements(propertyDefinition.DataType))
            {
                throw new ArgumentException($"Invalid object type {type} (expecting {propertyDefinition.DataType}) for {propertyDefinition.FilePropertyKey}");
            }
        }
    }
}