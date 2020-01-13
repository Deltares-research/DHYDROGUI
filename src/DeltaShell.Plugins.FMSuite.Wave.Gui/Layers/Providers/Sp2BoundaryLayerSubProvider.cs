using System.Collections.Generic;
using System.Drawing;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    // TODO: Remove once old boundaries are retired.
    public class Sp2BoundaryLayerSubProvider : Feature2DLayerSubProvider
    {
        public Sp2BoundaryLayerSubProvider(IWaveLayerFactory factory) : base(factory) {}

        private static readonly string modelName = typeof(WaveModel).Name;

        protected override bool IsCorrectFeatureSet(IEnumerable<Feature2D> features, IWaveModel model) =>
            Equals(features, model.Sp2Boundaries);

        protected override ILayer CreateFeatureLayer(IWaveModel model)
        {
            return new VectorLayer("Boundary from sp2")
            {
                DataSource = new Feature2DCollection().Init(model.Sp2Boundaries, "Sp2Boundary", modelName,
                                                            model.CoordinateSystem),
                Style = new VectorStyle
                {
                    Line = new Pen(Color.DarkOrange, 3f),
                    GeometryType = typeof(ILineString)
                },
                NameIsReadOnly = true,
                Selectable = false
            };
        }

    }
}