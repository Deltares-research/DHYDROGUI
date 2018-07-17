using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Properties;
using DelftTools.Utils;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Common.Layers
{
    public class SnappedLeveeBreachCollection : SnappedFeatureCollection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SnappedLeveeBreachCollection));

        public SnappedLeveeBreachCollection(IGridOperationApi operationApi, ICoordinateSystem coordinateSystem, IList originalFeatures, VectorStyle originalFeaturesLayerStyle, string layerName, string snapApiFeatureType) : base(operationApi, coordinateSystem, originalFeatures, originalFeaturesLayerStyle, layerName, snapApiFeatureType)
        {
        }


        protected override void CalculateSnappedFeatures()
        {
            SnappedFeatures.Clear();

            var leveeBreaches = OriginalFeatures.OfType<LeveeBreach>();

            foreach (var leveeBreach in leveeBreaches)
            {
                SnappedFeatures.Add(GetSnappedFeature(leveeBreach));
            }
        }

        protected override Feature2D GetSnappedFeature(IFeature feature, IGeometry snappedGeometry = null)
        {
            var leveeBreach = feature as LeveeBreach;

            IEnumerable<IGeometry> snappedLeveeBreachGeometry = new List<IGeometry>();
            

            if (leveeBreach != null)
            {
                if (snappedGeometry == null || snappedGeometry.IsEmpty)
                {
                    try
                    {
                        snappedLeveeBreachGeometry = OperationApi.GetGridSnappedGeometry(StructureRegion.StructureTypeName.LeveeBreach, new[] { leveeBreach.Geometry, leveeBreach.BreachLocation});
                        if (snappedLeveeBreachGeometry.Count() != 2)
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
                Log.WarnFormat(Resources.SnappedFeatureCollection_GetSnappedFeature_No_snapped_geometry_was_generated_for_type__0__, "LeveeBreaches (no levee breach type)");
            }

            var feature2D = new LeveeBreach();
            if (feature.Attributes != null)
                feature2D.Attributes = (IFeatureAttributeCollection)feature.Attributes.Clone();
            if (feature is INameable)
                feature2D.Name = ((INameable)feature).Name;

            if (snappedLeveeBreachGeometry.Count() == 2)
            {
                feature2D.Geometry = snappedLeveeBreachGeometry.First();
                var breachLocation = snappedLeveeBreachGeometry.Last() as IPoint;
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
