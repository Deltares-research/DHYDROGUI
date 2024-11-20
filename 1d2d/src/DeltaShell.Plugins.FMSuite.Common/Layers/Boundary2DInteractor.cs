using System;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Common.Layers
{
    internal class Boundary2DInteractor: Feature2DLineInteractor
    {
        public Boundary2DInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle,
            IEditableObject editableObject) : base(layer, feature, vectorStyle, editableObject)
        {
        }

        public Func<Feature2D, int, bool> AllowRemovePoint { private get; set; }

        public override bool RemoveTracker(TrackerFeature trackerFeature)
        {
            return trackerFeature != null &&
                   (AllowRemovePoint == null || AllowRemovePoint(SourceFeature as Feature2D, trackerFeature.Index)) &&
                   base.RemoveTracker(trackerFeature);
        }
    }
}
