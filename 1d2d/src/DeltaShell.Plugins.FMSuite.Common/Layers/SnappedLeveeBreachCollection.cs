using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Properties;
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

        public SnappedLeveeBreachCollection(IGridOperationApi operationApi, HydroArea area2D, IList originalFeatures, VectorStyle originalFeaturesLayerStyle, string layerName, string snapApiFeatureType) : base(operationApi, area2D, originalFeatures, originalFeaturesLayerStyle, layerName, snapApiFeatureType)
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

            leveeBreach = new LeveeBreach();
            if (feature.Attributes != null)
                leveeBreach.Attributes = (IFeatureAttributeCollection)feature.Attributes.Clone();
            if (feature is INameable nameable)
                leveeBreach.Name = nameable.Name;

            if (snappedLeveeBreachGeometry.Count() != 2) 
                return leveeBreach;

            leveeBreach.Geometry = snappedLeveeBreachGeometry.First();
            
            if (snappedLeveeBreachGeometry.Last() is IPoint breachLocation)
            {
                leveeBreach.BreachLocationX = breachLocation.X;
                leveeBreach.BreachLocationY = breachLocation.Y;
            }

            return leveeBreach;
        }
    }
}
