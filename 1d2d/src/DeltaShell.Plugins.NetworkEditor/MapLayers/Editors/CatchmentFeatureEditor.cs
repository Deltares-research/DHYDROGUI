using System;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors
{
    public class CatchmentFeatureEditor : DrainageBasinFeatureEditor
    {
        private readonly bool usingPointGeometry;

        public CatchmentFeatureEditor(bool usingPointGeometry = false)
        {
            this.usingPointGeometry = usingPointGeometry;
        }

        public override IFeature AddNewFeatureByGeometry(ILayer layer, IGeometry geometry)
        {
            if (geometry.Envelope is IPoint) //single click
            {
                geometry = new Point(geometry.Coordinate);
            }

            var editAction = new DefaultEditAction("Add new catchment");
            DrainageBasin.BeginEdit(editAction);
            try
            {
                var newCatchment = (Catchment) base.AddNewFeatureByGeometry(layer, geometry);
                newCatchment.CatchmentType = NewCatchmentType;
                newCatchment.Name = HydroNetworkHelper.GetUniqueFeatureName(DrainageBasin, newCatchment);
                return newCatchment;
            }
            catch (Exception)
            {
                editAction.Name += " (failed)";
                throw;
            }
            finally
            {
                DrainageBasin.EndEdit();
            }
        }

        public override IFeatureInteractor CreateInteractor(ILayer layer, IFeature feature)
        {
            return usingPointGeometry
                       ? (IFeatureInteractor) new CatchmentPointFeatureInteractor(layer, feature, ((VectorLayer)layer).Style, DrainageBasin) 
                       : new CatchmentFeatureInteractor(layer, feature, ((VectorLayer)layer).Style, DrainageBasin);
        }

        public CatchmentType NewCatchmentType { get; set; }
    }
}