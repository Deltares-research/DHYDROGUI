using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor
{
    public class CoordinateDataRow : CustomTypeDescriptor
    {
        private readonly ICollection<PropertyDescriptor> propertyDescriptors;
        private readonly IModelFeatureCoordinateData featureCoordinateData;
        private readonly int rowIndex;

        public CoordinateDataRow(IModelFeatureCoordinateData featureCoordinateData, int rowIndex, ICollection<PropertyDescriptor> propertyDescriptors)
        {
            this.propertyDescriptors = propertyDescriptors;
            this.featureCoordinateData = featureCoordinateData;
            this.rowIndex = rowIndex;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return new PropertyDescriptorCollection(propertyDescriptors.ToArray());
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        /// <summary>
        /// Gets the value for this row and the specified <param name="columnIndex"/>
        /// </summary>
        /// <param name="columnIndex">Column index for getting the value</param>
        /// <returns>Value for this row and the specified <param name="columnIndex"/></returns>
        public object GetDataValue(int columnIndex)
        {
            return featureCoordinateData.DataColumns[columnIndex].ValueList[rowIndex];
        }

        /// <summary>
        /// Set the value for this row and the specified <param name="columnIndex"/>
        /// </summary>
        /// <param name="columnIndex">Column index for setting the value</param>
        /// <param name="value">Value to set (must match with propertyDescriptor type)</param>
        public void SetDataValue(int columnIndex, object value)
        {
            featureCoordinateData.DataColumns[columnIndex].ValueList[rowIndex] = value;
        }

        /// <summary>
        /// Retrieves the geometry value for the specified <see cref="GeometryPropertyDescriptorType"/>
        /// </summary>
        /// <param name="type">Type of geometry data to retrieve</param>
        /// <returns>Coordinate value for this row</returns>
        public double GetCoordinateValue(GeometryPropertyDescriptorType type)
        {
            var geometryCoordinate = featureCoordinateData.Feature.Geometry.Coordinates[rowIndex];

            switch (type)
            {
                case GeometryPropertyDescriptorType.XValue:
                    return geometryCoordinate.X;
                case GeometryPropertyDescriptorType.YValue:
                    return geometryCoordinate.Y;
                case GeometryPropertyDescriptorType.ZValue:
                    return geometryCoordinate.Z;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
