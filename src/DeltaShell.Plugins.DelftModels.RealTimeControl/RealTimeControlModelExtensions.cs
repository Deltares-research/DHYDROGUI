using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.CoordinateSystems.Transformations;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    public static class RealTimeControlModelExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlModelExtensions));
        private static readonly IGeometryFactory GeometryFactory = new GeometryFactory();

        public static bool ToCoordinateSystem(this IRealTimeControlModel realTimeControlModel, ICoordinateTransformation transformation)
        {
            if (transformation == null || transformation.SourceCS == null)
            {
                Log.Error("Can not convert from an undefined coordinate system.");
                return false;
            }

            if (transformation.TargetCS == null)
            {
                Log.Error("Can not convert the model to an undefined target coordinate system.");
                return false;
            }

            // PROJ4 contains both authority and name of the coordinate system. 
            if (realTimeControlModel.CoordinateSystem == null || !realTimeControlModel.CoordinateSystem.PROJ4.Equals(((ICoordinateSystem)transformation.SourceCS).PROJ4))
            {
                Log.Error("The model's coordinate system is not equal to the source coordinate system of the given transformation.");
                return false;
            }

            var features = realTimeControlModel.GetAllItemsRecursive().OfType<IFeature>();
            if (!features.All(f => CanTransformFeatureGeometry(f, transformation)))
            {
                // Error message already given in that method. 
                return false;
            }

            // Will suspend layout changes. 
            realTimeControlModel.BeginEdit(new DefaultEditAction("Convert coordinate system"));

            try
            {
                realTimeControlModel.CoordinateSystem = (ICoordinateSystem)transformation.TargetCS;

                // Update already transformed geometries
                foreach (var feature in features)
                {
                    TransformGeometry(feature, transformation);
                }
            }
            finally
            {
                realTimeControlModel.EndEdit();
            }

            return true;
        }

        private static void TransformGeometry(IFeature feature, ICoordinateTransformation transformation)
        {
            feature.Geometry = GeometryTransform.TransformGeometry(GeometryFactory, feature.Geometry, transformation.MathTransform);
        }
        
        private static bool CanTransformFeatureGeometry(IFeature feature, ICoordinateTransformation transformation)
        {
            var transformedGeometry = GeometryTransform.TransformGeometry(GeometryFactory, feature.Geometry, transformation.MathTransform);
            /* 
             * If the coordinate transformation throws an exception, it will be caught, and the function will return null. 
             * Therefore, no try/catch, but a null check. Also, in some case, Infinities are returned for some transformation. 
             * These are seen as failed transformations as well. 
             */
            if (transformedGeometry != null && !IsInvalidCoordinate(transformedGeometry.Coordinate)) return true;

            Log.ErrorFormat("Can not convert feature {0} to the specified coordinate system.", feature);
            return false;
        }

        private static bool IsInvalidCoordinate(Coordinate coordinate)
        {
            return coordinate == null || IsInvalidNumber(coordinate.X) || IsInvalidNumber(coordinate.Y);
        }

        private static bool IsInvalidNumber(double value)
        {
            return double.IsInfinity(value) || double.IsNaN(value);
        }
    }
}