using System;
using System.ComponentModel;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor
{
    /// <summary>
    /// <see cref="PropertyDescriptor"/> for <see cref="CoordinateDataRow"/> that represents a <see cref="IDataColumn"/> value
    /// </summary>
    public class CoordinateDataRowPropertyDescriptor : PropertyDescriptor
    {
        private readonly string displayName;
        private readonly Type type;
        private readonly int columnIndex;

        public CoordinateDataRowPropertyDescriptor(string name, string displayName, Type type, int columnIndex) : base(name, null)
        {
            this.displayName = displayName;
            this.type = type;
            this.columnIndex = columnIndex;
        }

        public override string DisplayName
        {
            get
            {
                return displayName;
            }
        }

        public override Type ComponentType
        {
            get
            {
                return typeof(CoordinateDataRow);
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return type;
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return (component as CoordinateDataRow)?.GetDataValue(columnIndex);
        }

        public override void SetValue(object component, object value)
        {
            (component as CoordinateDataRow)?.SetDataValue(columnIndex, value);
        }

        public override void ResetValue(object component)
        {
            // Nothing to be done, enforced through PropertyDescriptor
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}