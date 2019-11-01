using System;
using System.Drawing;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using GeoAPI.Extensions.CoordinateSystems;
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

        /// <summary>
        /// Create a new obstacle layer from the obstacles within
        /// <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the obstacle features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
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

        /// <summary>
        /// Create a new obstacle data layer.
        /// </summary>
        /// <param name="obstacleData">The obstacle data.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the obstacle data features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the parameters is <c>null</c>.
        /// </exception>
        public static ILayer CreateObstacleDataLayer(EventedList<WaveObstacle> obstacleData, 
                                                     ICoordinateSystem coordinateSystem)
        {
            if (obstacleData == null)
            {
                throw new ArgumentNullException(nameof(obstacleData));
            }

            if (coordinateSystem == null)
            {
                throw new ArgumentNullException(nameof(coordinateSystem));
            }

            return new VectorLayer(WaveLayerNames.ObstacleDataLayerName)
            {
                DataSource = new Feature2DCollection().Init(obstacleData, 
                                                            "WaveObstacleData", 
                                                            waveModelName,
                                                            coordinateSystem),
                Style = new VectorStyle
                {
                    Symbol = WaveLayerIcons.ObstacleData,
                    GeometryType = typeof(IPoint)
                },
                NameIsReadOnly = true
            };
        }

        /// <summary>
        /// Create a new observation points layer from the observation points
        /// within <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the observation points features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateObservationPointsLayer(IWaveModel waveModel)
        {
            if (waveModel == null)
            {
                throw new ArgumentNullException(nameof(waveModel));
            }

            return new VectorLayer(WaveLayerNames.ObservationPointLayerName)
            {
                NameIsReadOnly = true,
                FeatureEditor = new Feature2DEditor(waveModel),
                Style = new VectorStyle
                {
                    GeometryType = typeof(IPoint),
                    Symbol = WaveLayerIcons.ObservationPoint
                },
                DataSource = new Feature2DCollection().Init(waveModel.ObservationPoints, 
                                                            "ObservationPoints",
                                                            waveModelName,
                                                            waveModel.CoordinateSystem),
            };
        }

        /// <summary>
        /// Create a new observation cross-section layer from the observation cross sections
        /// within <paramref name="waveModel"/>.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> visualising the observation cross-sections features.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateObservationCrossSectionLayer(IWaveModel waveModel)
        {
            if (waveModel == null)
            {
                throw new ArgumentNullException(nameof(waveModel));
            }

            return new VectorLayer(WaveLayerNames.ObservationCrossSectionLayerName)
            {
                DataSource = new Feature2DCollection().Init(waveModel.ObservationCrossSections, 
                                                            "CrS",
                                                            waveModelName,
                                                            waveModel.CoordinateSystem),
                FeatureEditor = new Feature2DEditor(waveModel),
                Style = new VectorStyle
                {
                    Line = new Pen(Color.LightSteelBlue, 3f),
                    GeometryType = typeof(ILineString)
                },
                NameIsReadOnly = true
            };
        }
    }
}