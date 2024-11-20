using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class HydroNodeInteractor : NodeInteractor
    {
        public HydroNodeInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject) : base(layer, feature, vectorStyle, editableObject)
        {
        }

        public override IEnumerable<IFeatureRelationInteractor> GetFeatureRelationInteractors(IFeature feature)
        {
            return HydroNetworkFeatureEditor.GetFeatureRelationInteractor(feature);
        }

        public override void Delete()
        {
            var node = (HydroNode)SourceFeature;

            var links = node.Links.ToArray();
            foreach (var link in links)
            {
                HydroRegion.RemoveLink(link);
            }

            base.Delete();
        }
    }
}