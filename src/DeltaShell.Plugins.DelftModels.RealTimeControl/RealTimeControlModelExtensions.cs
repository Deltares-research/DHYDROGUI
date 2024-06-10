using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
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
            if (realTimeControlModel.CoordinateSystem == null || !realTimeControlModel.CoordinateSystem.EqualsTo((ICoordinateSystem) transformation.SourceCS))
            {
                Log.Error("The model's coordinate system is not equal to the source coordinate system of the given transformation.");
                return false;
            }

            IEnumerable<IFeature> features = realTimeControlModel.GetAllItemsRecursive().OfType<IFeature>();
            if (!features.All(f => CanTransformFeatureGeometry(f, transformation)))
            {
                // Error message already given in that method. 
                return false;
            }

            // Will suspend layout changes. 
            realTimeControlModel.BeginEdit("Convert coordinate system");

            try
            {
                realTimeControlModel.CoordinateSystem = (ICoordinateSystem) transformation.TargetCS;

                // Update already transformed geometries
                foreach (IFeature feature in features)
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

        // DELFT3DFM-1441: Existing projects can have ControlGroups with the same names
        public static void MakeControlGroupNamesUnique(this RealTimeControlModel realTimeControlModel)
        {
            if (realTimeControlModel.ControlGroups.Select(cg => cg.Name).HasUniqueValues())
            {
                return;
            }

            NamingHelper.MakeNamesUnique(realTimeControlModel.ControlGroups);
            Log.WarnFormat(Resources.RealTimeControlModelExtensions_MakeControlGroupNamesUnique_ControlGroup_names_for_Model__0__were_not_unique__1_Control_Groups_have_been_renamed_such_that_they_are_now_unique_,
                           realTimeControlModel.Name, Environment.NewLine);
        }

        private static void TransformGeometry(IFeature feature, ICoordinateTransformation transformation)
        {
            feature.Geometry = GeometryTransform.TransformGeometry(GeometryFactory, feature.Geometry, transformation.MathTransform);
        }

        private static bool CanTransformFeatureGeometry(IFeature feature, ICoordinateTransformation transformation)
        {
            IGeometry transformedGeometry = GeometryTransform.TransformGeometry(GeometryFactory, feature.Geometry, transformation.MathTransform);
            /* 
             * If the coordinate transformation throws an exception, it will be caught, and the function will return null. 
             * Therefore, no try/catch, but a null check. Also, in some case, Infinities are returned for some transformation. 
             * These are seen as failed transformations as well. 
             */
            if (transformedGeometry != null && !IsInvalidCoordinate(transformedGeometry.Coordinate))
            {
                return true;
            }

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

        #region SyncControlGroupDataItemNames

        // DELFT3DFM-1441: Existing projects can have ControlGroup DataItems with ChildDataItems without the correct ControlGroup Name (as a prefix)
        public static void SyncControlGroupDataItemNames(this RealTimeControlModel realTimeControlModel)
        {
            if (!realTimeControlModel.ControlGroups.Any())
            {
                return;
            }

            realTimeControlModel.ControlGroups
                                .Where(cg => !realTimeControlModel.ControlGroupDataItemChildDataItemNamesAreInSync(cg))
                                .ForEach(realTimeControlModel.SyncControlGroupChildDataItemNames);
        }

        public static void SyncControlGroupChildDataItemNames(this RealTimeControlModel realTimeControlModel, IControlGroup controlGroup)
        {
            IDataItem controlGroupDataItem = realTimeControlModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup));
            if (controlGroupDataItem == null)
            {
                return;
            }

            if (!controlGroupDataItem.Children.Any())
            {
                return;
            }

            foreach (IDataItem child in controlGroupDataItem.Children.Where(di => di.Role.HasFlag(DataItemRole.Input)))
            {
                int postfixIndex = child.Name.IndexOf(RealTimeControlModel.InputPostFix, StringComparison.InvariantCulture);
                if (postfixIndex < 1)
                {
                    continue;
                }

                string oldControlGroupName = child.Name.Substring(0, postfixIndex);
                child.Name = child.Name.Replace(oldControlGroupName, controlGroup.Name);
            }

            foreach (IDataItem child in controlGroupDataItem.Children.Where(di => di.Role.HasFlag(DataItemRole.Output)))
            {
                int postfixIndex = child.Name.IndexOf(RealTimeControlModel.OutputPostFix, StringComparison.InvariantCulture);
                if (postfixIndex < 1)
                {
                    continue;
                }

                string oldControlGroupName = child.Name.Substring(0, postfixIndex);
                child.Name = child.Name.Replace(oldControlGroupName, controlGroup.Name);
            }
        }

        private static bool ControlGroupDataItemChildDataItemNamesAreInSync(this RealTimeControlModel realTimeControlModel, IControlGroup controlGroup)
        {
            IDataItem controlGroupDataItem = realTimeControlModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup));
            if (controlGroupDataItem == null)
            {
                return true;
            }

            if (!controlGroupDataItem.Children.Any())
            {
                return true;
            }

            if (controlGroupDataItem.Children
                                    .Where(di => di.Role.HasFlag(DataItemRole.Input))
                                    .Any(child => !child.Name.StartsWith(controlGroup.Name + RealTimeControlModel.InputPostFix)))
            {
                return false;
            }

            if (controlGroupDataItem.Children
                                    .Where(di => di.Role.HasFlag(DataItemRole.Output))
                                    .Any(child => !child.Name.StartsWith(controlGroup.Name + RealTimeControlModel.OutputPostFix)))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}