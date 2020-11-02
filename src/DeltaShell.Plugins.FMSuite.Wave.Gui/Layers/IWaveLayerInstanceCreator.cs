using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using SharpMap.Api;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers
{
    /// <summary>
    /// <see cref="IWaveLayerInstanceCreator"/> defines the methods to construct the
    /// different layers of the wave model.
    /// </summary>
    public interface IWaveLayerInstanceCreator
    {
        /// <summary>
        /// Creates a new model layer from the given <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>A new <see cref="ILayer"/> containing teh model.</returns>
        /// <exception cref="System.ArgumentNullException">
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
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="domain"/> is <c>null</c>.
        /// </exception>
        ILayer CreateWaveDomainDataLayer(WaveDomainData domain);

        /// <summary>
        /// Creates a new grid layer from the given <paramref name="discreteGrid"/>
        /// and <paramref name="coordinateSystem"/>.
        /// </summary>
        /// <param name="discreteGrid">The discrete grid.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>
        /// A new grid <see cref="ILayer"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="discreteGrid"/> is <c>null</c>.
        /// </exception>
        ILayer CreateGridLayer(IDiscreteGridPointCoverage discreteGrid,
                               ICoordinateSystem coordinateSystem);

        /// <summary>
        /// Creates a new obstacle layer from the obstacles within
        /// <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualizing the obstacle features.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        ILayer CreateObstacleLayer(IWaveModel waveModel);

        /// <summary>
        /// Creates a new observation points layer from the observation points
        /// within <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualizing the observation points features.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        ILayer CreateObservationPointsLayer(IWaveModel waveModel);

        /// <summary>
        /// Creates a new observation cross-section layer from the observation cross sections
        /// within <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualizing the observation cross-sections features.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        ILayer CreateObservationCrossSectionLayer(IWaveModel waveModel);

        /// <summary>
        /// Creates the boundary group layer.
        /// </summary>
        /// <param name="featuresProviderContainer">The features container.</param>
        /// <returns>
        /// A new boundary group layer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        ILayer CreateBoundaryLayer(IBoundaryMapFeaturesContainer featuresProviderContainer);

        /// <summary>
        /// Creates the support points layer.
        /// </summary>
        /// <param name="featureProvider">The support points feature provider.</param>
        /// <returns>
        /// A new support points layer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        ILayer CreateSupportPointsLayer(IFeatureProvider featureProvider);

        /// <summary>
        /// Creates the boundary line layer.
        /// </summary>
        /// <param name="featureProvider">The boundary line feature provider.</param>
        /// <returns>
        /// A new boundary line layer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        ILayer CreateBoundaryLineLayer(IFeatureProvider featureProvider);

        /// <summary>
        /// Creates the boundary start point layer.
        /// </summary>
        /// <param name="featureProvider">The boundary start point feature provider.</param>
        /// <returns>
        /// A new boundary start point layer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        ILayer CreateBoundaryStartPointLayer(IFeatureProvider featureProvider);

        /// <summary>
        /// Creates the boundary end point layer.
        /// </summary>
        /// <param name="featureProvider">The boundary end point feature provider.</param>
        /// <returns>
        /// A new boundary end point layer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        ILayer CreateBoundaryEndPointLayer(IFeatureProvider featureProvider);

        /// <summary>
        /// Creates the selected support point layer.
        /// </summary>
        /// <param name="featureProvider">The selected support point feature provider.</param>
        /// <returns>
        /// A new selected support point layer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        ILayer CreateSelectedSupportPointLayer(IFeatureProvider featureProvider);

        /// <summary>
        /// Creates the inactive support points layer.
        /// </summary>
        /// <param name="featureProvider">The inactive support points feature provider.</param>
        /// <returns>
        /// A new inactive support points layer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        ILayer CreateInactiveSupportPointsLayer(IFeatureProvider featureProvider);

        /// <summary>
        /// Creates the active support points layer.
        /// </summary>
        /// <param name="featureProvider">The active support points feature provider.</param>
        /// <returns>
        /// A new active support points layer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        ILayer CreateActiveSupportPointsLayer(IFeatureProvider featureProvider);

        /// <summary>
        /// Creates the wave output data layer.
        /// </summary>
        /// <param name="outputData">The output data.</param>
        /// <returns>
        /// A new wave output data layer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        ILayer CreateWaveOutputDataLayer(IWaveOutputData outputData);

        /// <summary>
        /// Creates a new output group layer with the given <paramref name="layerName"/>.
        /// </summary>
        /// <param name="layerName">Name of the domain.</param>
        /// <returns>
        /// A new output <see cref="ILayer"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="layerName"/> is <c>null</c>.
        /// </exception>
        ILayer CreateWaveOutputGroupLayer(string layerName);
    }
}