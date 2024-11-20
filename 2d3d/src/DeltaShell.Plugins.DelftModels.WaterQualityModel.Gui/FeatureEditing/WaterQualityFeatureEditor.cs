using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.FeatureEditing
{
    public class WaterQualityFeatureEditor : FeatureEditor
    {
        public override IFeatureInteractor CreateInteractor(ILayer layer, IFeature feature)
        {
            var vectorLayer = layer as VectorLayer;
            VectorStyle vectorStyle = vectorLayer != null ? vectorLayer.Style : null;

            if (feature.Geometry is IPoint)
            {
                return new FeaturePointInteractor(layer, feature, vectorStyle, null);
            }

            return base.CreateInteractor(layer, feature);
        }

        public override IFeature AddNewFeatureByGeometry(ILayer layer, IGeometry geometry)
        {
            IFeature addNewFeatureByGeometry = base.AddNewFeatureByGeometry(layer, geometry);
            var load = addNewFeatureByGeometry as WaterQualityLoad;
            if (load != null)
            {
                load.Z = double.NaN;
            }

            var observationPoint = addNewFeatureByGeometry as WaterQualityObservationPoint;
            if (observationPoint != null)
            {
                observationPoint.Z = double.NaN;
            }

            return addNewFeatureByGeometry;
        }
    }
}