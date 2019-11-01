using System;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers
{
    /// <summary>
    /// <see cref="WaveLayerFactory"/> provides the methods to construct the
    /// different layers of the wave model.
    /// </summary>
    public static class WaveLayerFactory
    {
        /// <summary> The wave model name. </summary>
        private static readonly string waveModelName = typeof(WaveModel).Name;

        /// <summary>
        /// Create a new model layer from the given <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>A new <see cref="ILayer"/> containing teh model.</returns>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateModelGroupLayer(WaveModel waveModel)
        {
            if (waveModel == null)
            {
                throw new ArgumentNullException(nameof(waveModel));
            }

            return new ModelGroupLayer
            {
                Name = waveModel.Name,
                Model = waveModel,
            };
        }

        /// <summary>
        /// Create a new <see cref="WaveDomainData"/> layer.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> of the <see cref="WaveDomainData"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="domain"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateWaveDomainDataLayer(WaveDomainData domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            return new GroupLayer(WaveLayerNames.GetDomainLayerName(domain.Name));
        }

        public static ILayer CreateObstacleLayer(IWaveModel waveModel)
        {
            if (waveModel == null)
            {
                throw new ArgumentNullException(nameof(waveModel));
            }

            return new VectorLayer(WaveLayerNames.ObstacleLayerName)
            {
                DataSource = new Feature2DCollection().Init(waveModel.Obstacles,
                                                            "Obstacle", 
                                                            waveModelName,
                                                            waveModel.CoordinateSystem),
                FeatureEditor = new Feature2DEditor(waveModel),
                Style = new VectorStyle
                {
                    Line = new Pen(Color.Red, 3f),
                    GeometryType = typeof(ILineString)
                },
                NameIsReadOnly = true
            };
        }
    }
}