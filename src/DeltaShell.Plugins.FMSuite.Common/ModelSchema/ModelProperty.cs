using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Aop;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;

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
        /// <param name="propertyDefinition">Property definition for this property.</param>
        /// <param name="valueAsString">String representing the initial value of this property.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="propertyDefinition"/> is null.</exception>
        /// <exception cref="FormatException">
        ///   When <paramref name="valueAsString"/> does not properly express the <see cref="ModelPropertyDefinition.DataType"/> 
        ///   specified in <see cref="PropertyDefinition"/>. Check <see cref="System.Exception.InnerException"/> for underlying cause.
        /// </exception>
        protected ModelProperty(ModelPropertyDefinition propertyDefinition, string valueAsString)
        {
            this.propertyDefinition = propertyDefinition;

            if (propertyDefinition == null)
                throw new ArgumentNullException("propertyDefinition");

            var inputString = string.IsNullOrEmpty(valueAsString)
                                  ? propertyDefinition.DefaultValueAsString
                                  : valueAsString;
            if (propertyDefinition.DataType == typeof (Steerable))
            {
                value = new Steerable();
            }
            SetValueAsString(inputString);
        }

        /// <summary>
        /// The value of the property.
        /// </summary>
        /// <exception cref="ArgumentException">When type of value does not conform to <see cref="ModelPropertyDefinition.DataType"/> specified in <see cref="PropertyDefinition"/>.</exception>
        public object Value
        {
            get { return value; }
            set
            {
                if (value != null)
                {
                    ThrowIfTypesDontMatch(value.GetType());
                }
                this.value = value;
            }
        }

        /// <summary>
        /// The minimum allowed value if defined (object type defined by <see cref="ModelPropertyDefinition.DataType"/>)
        /// or null if not set.
        /// </summary>
        public object MinValue
        {
            get
            {
                if (!string.IsNullOrEmpty(propertyDefinition.MinValueAsString))
                {
                    return DataTypeValueParser.FromString(propertyDefinition.MinValueAsString, propertyDefinition.DataType);
                }
                return null;
            }
        }

        /// <summary>
        /// The maximum allowed value if defined (object type defined by <see cref="ModelPropertyDefinition.DataType"/>)
        /// or null if not set.
        /// </summary>
        public object MaxValue
        {
            get
            {
                if (!string.IsNullOrEmpty(propertyDefinition.MaxValueAsString))
                {
                    return DataTypeValueParser.FromString(propertyDefinition.MaxValueAsString, propertyDefinition.DataType);
                }
                return null;
            }
        }

        /// <summary>
        /// The description and definition of this property.
        /// </summary>
        public ModelPropertyDefinition PropertyDefinition
        {
            get { return propertyDefinition; }
        }
        
        /// <summary>
        /// The linked model properties. 
        /// </summary>
        /// <example>This can be a property which defines default values for a property with an enum.</example>
        public ModelProperty LinkedModelProperty { get; set; }

        /// <summary>
        /// Sets <see cref="Value"/> using a string representation.
        /// </summary>
        /// <param name="valueAsString">String representation of the new <see cref="Value"/>.</param>
        /// <exception cref="FormatException">
        ///   When <paramref name="valueAsString"/> does not properly express the <see cref="ModelPropertyDefinition.DataType"/> 
        ///   specified in <see cref="PropertyDefinition"/>. Check <see cref="System.Exception.InnerException"/> for underlying cause.
        /// </exception>
        public void SetValueAsString(string valueAsString)
        {
            try
            {
                if (propertyDefinition.DataType == typeof (Steerable))
                {
                    SetValueAsStringForSteerable(valueAsString);
                }
                else
                {
                    Value = DataTypeValueParser.FromString(valueAsString, propertyDefinition.DataType);
                }
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is FormatException)
                {
                    throw new FormatException(String.Format("Unexpected value string \"{0}\" for property \"{1}\"",
                                                            valueAsString, PropertyDefinition.FilePropertyKey),
                                              e);
                }
                if (e is OverflowException)
                {
                    throw new FormatException(String.Format("Value string \"{0}\" is too large/small for property \"{1}\"",
                                                            valueAsString, PropertyDefinition.FilePropertyKey),
                                              e);
                }
                // Unexpected exception type, let it continue
                throw;
            }
        }

        /// <summary>
        /// Set value logic for when <see cref="Value"/> is of type <see cref="Steerable"/>.
        /// </summary>
        /// <param name="valueAsString"></param>
        /// <exception cref="ArgumentException">When <see cref="valueAsString"/> is an invalid file name.</exception>
        private void SetValueAsStringForSteerable(string valueAsString)
        {
            var steerableValue = (Steerable) value;

            if (valueAsString == "REALTIME") // It is then either driven externally...
            {
                steerableValue.Mode = SteerableMode.External;
                return;
            }

            var fileName = Path.GetFileName(valueAsString);
            if (FileUtils.IsValidFileName(fileName) && Path.GetExtension(fileName) == FileSuffices.BcFile || Path.GetExtension(fileName) == FileSuffices.TimFile)
            {
                steerableValue.TimeSeriesFilename = valueAsString;
                steerableValue.Mode = SteerableMode.TimeSeries;
                return;
            }

            var constantValue = DataTypeValueParser.FromString<double>(valueAsString);
            steerableValue.ConstantValue = constantValue;
            steerableValue.Mode = SteerableMode.ConstantValue;
        }

        /// <summary>
        /// Returns <see cref="Value"/> in string representation.
        /// </summary>
        public virtual string GetValueAsString()
        {
            return DataTypeValueParser.ToString(value, propertyDefinition.DataType);
        }

        /// <summary>
        /// Precondition check to for checking if a value type corresponds to <see cref="ModelPropertyDefinition"/>.<see cref="ModelPropertyDefinition.DataType"/>.
        /// </summary>
        /// <param name="type">Type to be checked.</param>
        /// <exception cref="ArgumentException">When <paramref name="type"/> does not correspond.</exception>
        private void ThrowIfTypesDontMatch(Type type)
        {
            if (type != propertyDefinition.DataType && !type.Implements(propertyDefinition.DataType))
            {
                throw new ArgumentException(String.Format("Invalid object type {0} (expecting {1}) for {2}",
                                                  type, propertyDefinition.DataType,
                                                  propertyDefinition.FilePropertyKey));
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} ({2})", PropertyDefinition.Caption, GetValueAsString(),
                                 PropertyDefinition.Category);
        }

        public abstract object Clone();

        /// <summary>
        /// Checks if <see cref="Value"/> is valid.
        /// </summary>
        /// <returns>True if valid; False otherwise.</returns>
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

            return !(MaxValue is IComparable maxValue) || maxValue.CompareTo(value) >= 0;
        }

        /// <summary>
        /// Indicates if this property is enabled or not.
        /// </summary>
        /// <param name="properties">All available properties indexed on <see cref="ModelPropertyDefinition.FilePropertyKey"/>.</param>
        /// <returns>True if enabled; False otherwise.</returns>
        public bool IsEnabled(IEnumerable<ModelProperty> properties)
        {
            return propertyDefinition.IsEnabled(properties);
        }

        public bool IsVisible(IEnumerable<ModelProperty> properties)
        {
            return propertyDefinition.IsVisible(properties);
        }
    }
}