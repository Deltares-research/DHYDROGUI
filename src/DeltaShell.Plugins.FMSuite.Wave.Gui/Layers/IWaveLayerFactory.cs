using System;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers
{
    /// <summary>
    /// <see cref="IWaveLayerFactory"/> defines the methods to construct the
    /// different layers of the wave model.
    /// </summary>
    public interface IWaveLayerFactory
    {
        /// <summary>
        /// Creates a new model layer from the given <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>A new <see cref="ILayer"/> containing teh model.</returns>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        ILayer CreateModelGroupLayer(WaveModel waveModel);

        /// <summary>
        /// Creates a new <see cref="WaveDomainData"/> layer.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> of the <see cref="WaveDomainData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="domain"/> is <c>null</c>.
        /// </exception>
        ILayer CreateWaveDomainDataLayer(WaveDomainData domain);

        /// <summary>
        /// Creates a new snapped features layer from the <paramref name="snappedFeatures"/>.
        /// </summary>
        /// <param name="snappedFeatures">The snapped features.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the snapped features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="snappedFeatures"/> is <c>null</c>.
        /// </exception>
        ILayer CreateSnappedFeaturesLayer(WaveSnappedFeaturesGroupLayerData snappedFeatures);

        /// <summary>
        /// Creates a new output layer with the given <paramref name="domainName"/>.
        /// </summary>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="overrideLayerName">if set to <c>true</c> use the <paramref name="domainName"/> verbatim.</param>
        /// <returns>
        /// A new output <see cref="ILayer"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="domainName"/> is <c>null</c>.
        /// </exception>
        ILayer CreateOutputLayer(string domainName, bool overrideLayerName = false);

        /// <summary>
        /// Creates a new grid layer from the given <paramref name="discreteGrid"/>
        /// and <paramref name="coordinateSystem"/>.
        /// </summary>
        /// <param name="discreteGrid">The discrete grid.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>
        /// A new grid <see cref="ILayer"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="discreteGrid"/> is <c>null</c>.
        /// </exception>
        ILayer CreateGridLayer(IDiscreteGridPointCoverage discreteGrid,
                               ICoordinateSystem coordinateSystem);

        /// <summary>
        /// Creates a new obstacle data layer.
        /// </summary>
        /// <param name="obstacleData">The obstacle data.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the obstacle data features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the parameters is <c>null</c>.
        /// </exception>
        ILayer CreateObstacleDataLayer(IEventedList<WaveObstacle> obstacleData,
                                       ICoordinateSystem coordinateSystem);

        /// <summary>
        /// Creates a new obstacle layer from the obstacles within
        /// <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the obstacle features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        ILayer CreateObstacleLayer(IWaveModel waveModel);

        /// <summary>
        /// Creates a new observation points layer from the observation points
        /// within <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the observation points features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        ILayer CreateObservationPointsLayer(IWaveModel waveModel);

        /// <summary>
        /// Creates a new observation cross-section layer from the observation cross sections
        /// within <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the observation cross-sections features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        ILayer CreateObservationCrossSectionLayer(IWaveModel waveModel);
    }
}