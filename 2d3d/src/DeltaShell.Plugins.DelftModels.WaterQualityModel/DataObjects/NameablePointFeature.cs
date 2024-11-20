using System;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects
{
    [Entity]
    public abstract class NameablePointFeature : Feature, INameable
    {
        protected NameablePointFeature()
        {
            base.Geometry = new Point(0.0, 0.0);
            Name = string.Empty;
        }

        public override IGeometry Geometry
        {
            get => base.Geometry;
            set
            {
                if (!(value is IPoint))
                {
                    throw new NotSupportedException("Only point geometries are supported");
                }

                base.Geometry = value;
            }
        }

        [FeatureAttribute(Order = 1)]
        public virtual double X
        {
            get => PointGeometry.X;
            set => UpdatePointGeometry(value, PointGeometry.Y, PointGeometry.Z);
        }

        [FeatureAttribute(Order = 2)]
        public virtual double Y
        {
            get => PointGeometry.Y;
            set => UpdatePointGeometry(PointGeometry.X, value, PointGeometry.Z);
        }

        [FeatureAttribute(Order = 3)]
        public virtual double Z
        {
            get => PointGeometry.Z;
            set => UpdatePointGeometry(PointGeometry.X, PointGeometry.Y, value);
        }

        [FeatureAttribute(Order = 0)]
        public virtual string Name { get; set; }

        private IPoint PointGeometry => Geometry as IPoint;

        private void UpdatePointGeometry(double newX, double newY, double newZ)
        {
            Geometry = new Point(newX, newY, newZ);
        }
    }
}