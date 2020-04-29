using System;
using System.Collections.Generic;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="ObservationCrossSectionLayerSubProvider"/> implements
    /// <see cref="Feature2DLayerSubProvider"/> for <see cref="WaveModel.ObservationCrossSections"/>.
    /// </summary>
    /// <seealso cref="Feature2DLayerSubProvider" />
    public class ObservationCrossSectionLayerSubProvider : Feature2DLayerSubProvider
    {
        /// <summary>
        /// Creates a new <see cref="ObservationCrossSectionLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory"> The factory to create the layers with. </param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
        public ObservationCrossSectionLayerSubProvider(IWaveLayerFactory factory) : base(factory) {}

        protected override bool IsCorrectFeatureSet(IEnumerable<Feature2D> features, IWaveModel model) =>
            Equals(features, model.ObservationCrossSections);

        protected override ILayer CreateFeatureLayer(IWaveModel model) =>
            Factory.CreateObservationCrossSectionLayer(model);
    }
}