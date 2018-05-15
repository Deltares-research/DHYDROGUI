using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Properties;
using DelftTools.Utils;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Common.Layers
{
    public class SnappedDamBreakCollection: SnappedFeatureCollection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SnappedDamBreakCollection));

        public SnappedDamBreakCollection(IGridOperationApi operationApi, ICoordinateSystem coordinateSystem, IList originalFeatures, VectorStyle originalFeaturesLayerStyle, string layerName, string snapApiFeatureType) : base(operationApi, coordinateSystem, originalFeatures, originalFeaturesLayerStyle, layerName, snapApiFeatureType)
        {
        }


        protected override void CalculateSnappedFeatures()
        {
            SnappedFeatures.Clear();

            var lstDamBreaks = OriginalFeatures.OfType<DamBreak>();

            foreach (var damBreak in lstDamBreaks)
            {
                SnappedFeatures.Add(GetSnappedFeature(damBreak));
            }
        }

        protected override Feature2D GetSnappedFeature(IFeature feature, IGeometry snappedGeometry = null)
        {
            var damBreak = feature as DamBreak;

            IEnumerable<IGeometry> snappedDamBreakGeometry = new List<IGeometry>();
            

            if (damBreak != null)
            {
                if (snappedGeometry == null || snappedGeometry.IsEmpty)
                {
                    try
                    {
                        snappedDamBreakGeometry = OperationApi.GetGridSnappedGeometry("dambreak", new[] { damBreak.Geometry, damBreak.BreachLocation});
                        if (snappedDamBreakGeometry.Count() != 2)
                        {
                            Log.WarnFormat(Resources.SnappedFeatureCollection_GetSnappedFeature_No_snapped_geometry_was_generated_for_type__0__, feature.Geometry.GeometryType);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else
            {
                Log.WarnFormat(Resources.SnappedFeatureCollection_GetSnappedFeature_No_snapped_geometry_was_generated_for_type__0__, "DamBreak (no dam break type)");
            }

            var feature2D = new DamBreak();
            if (feature.Attributes != null)
                feature2D.Attributes = (IFeatureAttributeCollection)feature.Attributes.Clone();
            if (feature is INameable)
                feature2D.Name = ((INameable)feature).Name;

            if (snappedDamBreakGeometry.Count() == 2)
            {
                feature2D.Geometry = snappedDamBreakGeometry.First();
                var breachLocation = snappedDamBreakGeometry.Last() as IPoint;
                if (breachLocation != null)
                {
                    feature2D.BreachLocationX = breachLocation.X;
                    feature2D.BreachLocationY = breachLocation.Y;
                }
            }

            return feature2D;
        }
    }
}
