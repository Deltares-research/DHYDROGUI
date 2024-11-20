using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using Deltares.Infrastructure.API.Guards;
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
        private readonly IWaveLayerInstanceCreator instanceCreator;
        private readonly Func<IEnumerable<WaveModel>> getWaveModelsFunc;

        /// <summary>
        /// Creates a new <see cref="DiscreteGridPointCoverageLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator">The factory to build the layers with.</param>
        /// <param name="getWaveModelsFunc"> Function to retrieve the WaveModels. </param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="instanceCreator"/> or
        /// <paramref name="getWaveModelsFunc"/> is <c>null</c>.
        /// </exception>
        public DiscreteGridPointCoverageLayerSubProvider(IWaveLayerInstanceCreator instanceCreator,
                                                         Func<IEnumerable<WaveModel>> getWaveModelsFunc)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            Ensure.NotNull(getWaveModelsFunc, nameof(getWaveModelsFunc));

            this.instanceCreator = instanceCreator;
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
            return instanceCreator.CreateGridLayer(discreteGrid, coordinateSystem);
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