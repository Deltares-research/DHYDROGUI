using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using GeoAPI.Geometries;
using SharpMap.Converters.WellKnownText;

namespace DelftTools.Hydro.Helpers
{
    /// <summary>
    /// werkt niet... bijna wel!
    /// </summary>
    public class CoordinateConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }
        
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            Coordinate coordinate = (Coordinate)value;
            //var geometry = Geometry.DefaultFactory.CreatePoint(coordinate);
            
            //return geometry.AsText();
            return new SimpleCoordinate { X = coordinate.X, Y = coordinate.Y};
        }

        class SimpleCoordinate
        {
            public double X { get; set; }
            public double Y { get; set; }
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            if (sourceType == typeof(SimpleCoordinate))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string geometryString = (string)value;
                    return GeometryFromWKT.Parse(geometryString).Coordinate;
                }
                catch
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type coordinate");
                }
            }

            if (value is SimpleCoordinate simpleCoordinate)
            {
                try
                {
                    return new Coordinate(simpleCoordinate.X, simpleCoordinate.Y);
                }
                catch
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type coordinate");
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}