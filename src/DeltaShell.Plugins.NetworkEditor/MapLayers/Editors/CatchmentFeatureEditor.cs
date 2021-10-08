using System;
using System.Linq;
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
                //todo: at some point we want to call CancelEdit, but we need to test that etc
                editAction.Name += " (failed)";
                throw;
            }
            finally
            {
                DrainageBasin.EndEdit();
            }
        }

        protected override void AddFeatureToDataSource(ILayer layer, IFeature feature)
        {
            var catchment = (Catchment) feature;
            if (feature.Geometry.Envelope is IPoint) //single click
            {
                //find catchments under click
                var potentialParent = layer.DataSource.Features.OfType<Catchment>()
                                           .FirstOrDefault(c => c.CatchmentType.SubCatchmentTypes.Contains(NewCatchmentType) &&
                                                                c.Geometry.Contains(feature.Geometry));
                if (potentialParent != null)
                {
                    if (potentialParent.SubCatchments.Any(c => c.CatchmentType.Equals(NewCatchmentType)))
                    {
                        throw new InvalidOperationException("Parent catchment can have only one of each subtype");
                    }

                    potentialParent.SubCatchments.Add(catchment);
                    return;
                }
                throw new InvalidOperationException(
                    "Catchment directly on basin must be defined by at least three vertices"); //aborts the add
            }

            base.AddFeatureToDataSource(layer, feature);
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