using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class RunoffBoundaryFeatureInteractor : PointInteractor, INetworkFeatureInteractor
    {
        public RunoffBoundaryFeatureInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IDrainageBasin basin)
            : base(layer, feature, vectorStyle, basin)
        {
            DrainageBasin = basin;
        }

        private IDrainageBasin DrainageBasin { get; set; }

        public override IEnumerable<IFeatureRelationInteractor> GetFeatureRelationInteractors(IFeature feature)
        {
            yield return new HydroObjectToHydroLinkRelationInteractor();
        }

        protected override bool AllowDeletionCore()
        {
            return true;
        }

        protected override bool AllowMoveCore()
        {
            return true;
        }

        public override void Add(IFeature feature)
        {
            DrainageBasin.Boundaries.Add(feature as RunoffBoundary);
        }

        public override void Delete()
        {
            var plant = (RunoffBoundary)SourceFeature;

            var links = plant.Links.ToArray();
            foreach (var link in links)
            {
                HydroRegion.RemoveLink(link);
            }

            DrainageBasin.Boundaries.Remove(plant);
        }

        public INetwork Network { get; set; }
    }
}