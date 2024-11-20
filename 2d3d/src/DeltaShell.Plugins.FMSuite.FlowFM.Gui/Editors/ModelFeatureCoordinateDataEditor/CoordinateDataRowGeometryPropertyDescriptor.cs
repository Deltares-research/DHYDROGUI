using System;
using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor
{
    /// <summary>
    /// <see cref="PropertyDescriptor"/> for <see cref="CoordinateDataRow"/> that represents a coordinate value (
    /// <see cref="GeometryPropertyDescriptorType"/>)
    /// </summary>
    public class CoordinateDataRowGeometryPropertyDescriptor : PropertyDescriptor
    {
        public CoordinateDataRowGeometryPropertyDescriptor(string name) : base(name, null) {}

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
                return true;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return typeof(double);
            }
        }

        public GeometryPropertyDescriptorType Type { get; set; }

        public override bool CanResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object component)
        {
            return (component as CoordinateDataRow)?.GetCoordinateValue(Type);
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}