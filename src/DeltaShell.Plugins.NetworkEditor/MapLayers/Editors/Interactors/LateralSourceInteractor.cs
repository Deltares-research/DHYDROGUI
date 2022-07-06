using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    /// <summary>
    /// Editor used for lateral sources that are diffuse
    /// </summary>
    public class LateralSourceInteractor : BranchFeatureInteractor<LateralSource>
    {
        public LateralSourceInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject) : base(layer, feature, vectorStyle, editableObject) {}

        public override void Stop(SnapResult snapResult)
        {
            var sourceFeature = (IBranchFeature) SourceFeature;

            IPipe pipe = GetNearestPipe(sourceFeature);
            if (pipe == null)
            {
                base.Stop(snapResult);
                return;
            }

            sourceFeature.Branch = pipe;
            sourceFeature.Chainage = GetChainage(pipe, sourceFeature);

            base.Stop(snapResult);
        }

        public override IEnumerable<IFeatureRelationInteractor> GetFeatureRelationInteractors(IFeature feature)
        {
            return HydroNetworkFeatureEditor.GetFeatureRelationInteractor(feature);
        }

        public override void Delete()
        {
            var lateralSource = (LateralSource) SourceFeature;

            HydroLink[] links = lateralSource.Links.ToArray();
            foreach (HydroLink link in links)
            {
                HydroRegion.RemoveLink(link);
            }

            base.Delete();
        }

        private static double GetChainage(IBranch branch, IFeature sourceFeature)
        {
            double distance = GeometryHelper.Distance((ILineString) branch.Geometry,
                                                      sourceFeature.Geometry.Coordinate);
            if (branch.IsLengthCustom)
            {
                distance *= branch.Length / branch.Geometry.Length;
            }

            return BranchFeature.SnapChainage(branch.Length, distance);
        }

        private IPipe GetNearestPipe(IFeature sourceFeature)
        {
            return (IPipe) NetworkHelper.GetNearestBranch(((IHydroNetwork) Network).Pipes, sourceFeature.Geometry, 0.1);
        }
    }
}