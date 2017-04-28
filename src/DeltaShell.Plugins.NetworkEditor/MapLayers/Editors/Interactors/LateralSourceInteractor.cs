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
    /// <summary>
    /// Editor used for lateral sources that are diffuse 
    /// </summary>
    public class LateralSourceInteractor : BranchFeatureInteractor<LateralSource>
    {
        public LateralSourceInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject) : base(layer, feature, vectorStyle, editableObject)
        {
        }

        public override IEnumerable<IFeatureRelationInteractor> GetFeatureRelationInteractors(IFeature feature)
        {
            // HACK, TODO: remove this hack, ugly delegate to create related interactors is used in IRelatedFeatureInteractor
            return HydroNetworkFeatureEditor.GetFeatureRelationInteractor(feature);
        }

        public override void Delete()
        {
            var lateralSource = (LateralSource)SourceFeature;

            var links = lateralSource.Links.ToArray();
            foreach (var link in links)
            {
                HydroRegion.RemoveLink(link);
            }

            base.Delete();
        }
    }
}