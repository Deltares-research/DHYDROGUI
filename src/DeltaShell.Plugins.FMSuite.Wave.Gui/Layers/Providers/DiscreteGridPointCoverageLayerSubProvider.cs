using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="DiscreteGridPointCoverageLayerSubProvider"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="IDiscreteGridPointCoverage"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    public class DiscreteGridPointCoverageLayerSubProvider : ILayerSubProvider
    {
        private readonly IWaveLayerFactory factory;
        private readonly Func<IEnumerable<WaveModel>> getWaveModelsFunc;

        /// <summary>
        /// Creates a new <see cref="DiscreteGridPointCoverageLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory to build the layers with.</param>
        /// <param name="getWaveModelsFunc"> Function to retrieve the WaveModels. </param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="factory"/> or
        /// <paramref name="getWaveModelsFunc"/> is <c>null</c>.
        /// </exception>
        public DiscreteGridPointCoverageLayerSubProvider(IWaveLayerFactory factory,
                                                         Func<IEnumerable<WaveModel>> getWaveModelsFunc)
        {
            Ensure.NotNull(factory, nameof(factory));
            Ensure.NotNull(getWaveModelsFunc, nameof(getWaveModelsFunc));

            this.factory = factory;
            this.getWaveModelsFunc = getWaveModelsFunc;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is IDiscreteGridPointCoverage;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            if (!(sourceData is IDiscreteGridPointCoverage discreteGrid))
            {
                return null;
            }

            ICoordinateSystem coordinateSystem = GetGridCoordinateSystem(parentData, discreteGrid);
            return factory.CreateGridLayer(discreteGrid, coordinateSystem);
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }

        private ICoordinateSystem GetGridCoordinateSystem(object parent, IDiscreteGridPointCoverage discreteGrid)
        {
            if (parent is IWaveModel model)
            {
                return model.CoordinateSystem;
            }

            if (parent is WavmFileFunctionStore)
            {
                return null;
            }

            WaveModel ownerWaveModel = getWaveModelsFunc.Invoke()
                                                        .FirstOrDefault(w => w.GetAllItemsRecursive()
                                                                              .Contains(discreteGrid));
            return ownerWaveModel?.CoordinateSystem;
        }
    }
}