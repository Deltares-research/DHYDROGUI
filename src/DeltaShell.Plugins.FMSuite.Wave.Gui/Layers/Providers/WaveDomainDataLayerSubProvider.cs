using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="WaveDomainDataLayerSubProvider"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="WaveDomainData"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    public class WaveDomainDataLayerSubProvider : ILayerSubProvider
    {
        private readonly IWaveLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="WaveDomainDataLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator">The factory to build the layers with.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Throw when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public WaveDomainDataLayerSubProvider(IWaveLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is WaveDomainData;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return sourceData is WaveDomainData domainData
                       ? instanceCreator.CreateWaveDomainDataLayer(domainData)
                       : null;
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is WaveDomainData domainData))
            {
                yield break;
            }

            yield return domainData.Grid;
            yield return domainData.Bathymetry;
        }
    }
}