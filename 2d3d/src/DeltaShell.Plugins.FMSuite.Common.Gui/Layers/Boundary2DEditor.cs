using System;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Layers
{
    public class Boundary2DEditor : Feature2DEditor
    {
        public Boundary2DEditor(IEditableObject editableObject) : base(editableObject) { }

        public Func<Feature2D, int, bool> AllowRemovePoint { private get; set; }

        public override IFeatureInteractor CreateInteractor(ILayer layer, IFeature feature)
        {
            if (feature.Geometry is ILineString)
            {
                var vectorLayer = layer as VectorLayer;
                VectorStyle vectorStyle = vectorLayer != null ? vectorLayer.Style : null;
                return new Boundary2DInteractor(layer, feature, vectorStyle, EditableObject) { AllowRemovePoint = AllowRemovePoint };
            }

            return base.CreateInteractor(layer, feature);
        }
    }
}