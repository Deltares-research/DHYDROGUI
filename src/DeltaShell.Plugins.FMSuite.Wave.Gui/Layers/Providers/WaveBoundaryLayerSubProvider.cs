using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    // TODO: Remove once old boundaries are retired.
    public sealed class WaveBoundaryLayerSubProvider : Feature2DLayerSubProvider
    {
        public WaveBoundaryLayerSubProvider(IWaveLayerFactory factory) : base(factory) {}

        private static readonly string modelName = typeof(WaveModel).Name;

        protected override bool IsCorrectFeatureSet(IEnumerable<Feature2D> features, IWaveModel model) =>
            Equals(features, model.Boundaries);

        protected override ILayer CreateFeatureLayer(IWaveModel model)
        {
            return new VectorLayer(WaveLayerNames.BoundaryLayerName)
            {
                DataSource = new Feature2DCollection().Init(model.Boundaries, "Boundary", modelName,
                                                            model.CoordinateSystem, model.GetGridSnappedBoundary),
                FeatureEditor = new Feature2DEditor(model),
                Style = WaveModelLayerStyles.BoundaryStyle,
                NameIsReadOnly = true,
            };
        }

    }
}