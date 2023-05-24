using DelftTools.Hydro.Helpers;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors
{
    public class RunoffBoundaryFeatureEditor : DrainageBasinFeatureEditor
    {
        public override IFeature AddNewFeatureByGeometry(ILayer layer, IGeometry geometry)
        {
            DrainageBasin.BeginEdit(new DefaultEditAction("Add new runoff boundary"));
            try
            {
                var feat = base.AddNewFeatureByGeometry(layer, geometry);

                if (feat is INameable nameableFeature)
                {
                    nameableFeature.Name = HydroNetworkHelper.GetUniqueFeatureName(DrainageBasin, nameableFeature);
                }

                return feat;
            }
            finally
            {
                DrainageBasin.EndEdit();
            }
        }

        public override IFeatureInteractor CreateInteractor(ILayer layer, IFeature feature)
        {
            return new RunoffBoundaryFeatureInteractor(layer, feature, ((VectorLayer)layer).Style, DrainageBasin);
        }
    }
}