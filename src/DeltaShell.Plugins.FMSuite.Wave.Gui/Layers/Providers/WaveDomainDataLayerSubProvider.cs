using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Common;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="WaveDomainDataLayerSubProvider"/> implements the
    /// <see cref="IWaveLayerSubProvider"/> for data of type <see cref="WaveDomainData"/>.
    /// </summary>
    /// <seealso cref="IWaveLayerSubProvider" />
    public class WaveDomainDataLayerSubProvider : IWaveLayerSubProvider
    {
        private readonly IWaveLayerFactory factory;

        /// <summary>
        /// Creates a new <see cref="WaveDomainDataLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory to build the layers with.</param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
        public WaveDomainDataLayerSubProvider(IWaveLayerFactory factory)
        {
            Ensure.NotNull(factory, nameof(factory));
            this.factory = factory;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is WaveDomainData;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return sourceData is WaveDomainData domainData
                       ? factory.CreateWaveDomainDataLayer(domainData)
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