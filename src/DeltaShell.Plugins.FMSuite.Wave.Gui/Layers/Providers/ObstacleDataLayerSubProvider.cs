using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="ObstacleDataLayerSubProvider"/> implements the
    /// <see cref="IWaveLayerSubProvider"/> for data of type <see cref="IEventedList{WaveObstacle}"/>.
    /// </summary>
    /// <seealso cref="IWaveLayerSubProvider" />
    public class ObstacleDataLayerSubProvider : IWaveLayerSubProvider
    {
        private readonly IWaveLayerFactory factory;

        /// <summary>
        /// Creates a new <see cref="DiscreteGridPointCoverageLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory to build the layers with.</param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
        public ObstacleDataLayerSubProvider(IWaveLayerFactory factory)
        {
            Ensure.NotNull(factory, nameof(factory));
            this.factory = factory;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is IEventedList<WaveObstacle> &&
                   parentData is IWaveModel;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return sourceData is IEventedList<WaveObstacle> obstacles &&
                   parentData is IWaveModel model
                       ? factory.CreateObstacleDataLayer(obstacles, model.CoordinateSystem)
                       : null;
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }
    }
}