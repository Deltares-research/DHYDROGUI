using System;
using System.Collections.Generic;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="ObservationPointLayerSubProvider"/> implements
    /// <see cref="Feature2DLayerSubProvider"/> for <see cref="WaveModel.ObservationPoints"/>.
    /// </summary>
    /// <seealso cref="Feature2DLayerSubProvider" />
    public class ObservationPointLayerSubProvider : Feature2DLayerSubProvider
    {
        /// <summary>
        /// Creates a new instance of <see cref="ObservationPointLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory"> The factory to create the layers with. </param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
        public ObservationPointLayerSubProvider(IWaveLayerFactory factory) : base(factory) {}

        protected override bool IsCorrectFeatureSet(IEnumerable<Feature2D> features, IWaveModel model) =>
            Equals(features, model.ObservationPoints);

        protected override ILayer CreateFeatureLayer(IWaveModel model) =>
            Factory.CreateObservationPointsLayer(model);
    }
}