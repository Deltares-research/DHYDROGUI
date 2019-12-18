using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    /// <summary>
    /// <see cref="WaveModelMapLayerProvider"/> provides the layers of the Wave plugin.
    /// </summary>
    /// <seealso cref="IMapLayerProvider" />
    public class WaveModelMapLayerProvider : IMapLayerProvider
    {
        private static readonly string modelName = typeof(WaveModel).Name;

        // TODO this need to be further split up.
        /// <summary>
        /// Create the layer associated with the <paramref name="data"/> and
        /// <paramref name="parent"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="parent">The parent.</param>
        /// <returns>
        /// A new <see cref="ILayer"/> if it could be created; <c>null</c> otherwise.
        /// </returns>
        public ILayer CreateLayer(object data, object parent)
        {
            if (data is WaveModel waveModel)
            {
                return WaveLayerFactory.CreateModelGroupLayer(waveModel);
            }

            if (data is WaveDomainData domainData)
            {
                return WaveLayerFactory.CreateWaveDomainDataLayer(domainData);
            }

            if (data is WaveSnappedFeaturesGroupLayerData snappedFeaturesGroupLayerData)
            {
                return WaveLayerFactory.CreateSnappedFeaturesLayer(snappedFeaturesGroupLayerData);
            }

            var model = parent as IWaveModel;

            if (data is WavmFileFunctionStore wavmFileFunctionStore &&
                wavmFileFunctionStore.Functions.Count != 0)
            {
                return GetOutputLayer(wavmFileFunctionStore, model);
            }

            if (data is IDiscreteGridPointCoverage discreteGrid)
            {
                ICoordinateSystem coordinateSystem = GetGridCoordinateSystem(parent, model, discreteGrid);
                return WaveLayerFactory.CreateGridLayer(discreteGrid, coordinateSystem);
            }

            // Model dependent layers
            if (model == null)
            {
                return null;
            }

            if (data is IEventedList<WaveBoundaryCondition> boundaryConditions)
            {
                return new VectorLayer(WaveLayerNames.BoundaryConditionLayerName)
                {
                    DataSource =
                        new Feature2DCollection().Init(boundaryConditions, "BoundaryCondition", modelName,
                                                       model.CoordinateSystem),
                    Style = new VectorStyle
                    {
                        Symbol = WaveLayerIcons.CoordinateBasedBoundary,
                        GeometryType = typeof(IPoint)
                    },
                    NameIsReadOnly = true
                };
            }

            if (data is EventedList<WaveObstacle> obstacleData)
            {
                return WaveLayerFactory.CreateObstacleDataLayer(obstacleData, model.CoordinateSystem);
            }

            if (data is IEnumerable<Feature2D> features) { 

                if (Equals(features, model.Boundaries))
                {
                    return new VectorLayer(WaveLayerNames.BoundaryLayerName)
                    {
                        DataSource =
                            new Feature2DCollection().Init(model.Boundaries, "Boundary", modelName,
                                                           model.CoordinateSystem, model.GetGridSnappedBoundary),
                        FeatureEditor = new Feature2DEditor(model),
                        Style = WaveModelLayerStyles.BoundaryStyle,
                        NameIsReadOnly = true,
                        Selectable = !model.BoundaryIsDefinedBySpecFile
                    };
                }

                if (Equals(features, model.Sp2Boundaries))
                {
                    return new VectorLayer("Boundary from sp2")
                    {
                        DataSource =
                            new Feature2DCollection().Init(model.Sp2Boundaries, "Sp2Boundary", modelName,
                                                           model.CoordinateSystem),
                        Style = new VectorStyle
                        {
                            Line = new Pen(Color.DarkOrange, 3f),
                            GeometryType = typeof(ILineString)
                        },
                        NameIsReadOnly = true,
                        Selectable = false
                    };
                }

                if (Equals(features, model.Obstacles))
                {
                    return WaveLayerFactory.CreateObstacleLayer(model);
                }

                if (Equals(features, model.ObservationCrossSections))
                {
                    return WaveLayerFactory.CreateObservationCrossSectionLayer(model);
                }

                if (Equals(features, model.ObservationPoints))
                {
                    return WaveLayerFactory.CreateObservationPointsLayer(model);
                }
            }

            // TODO: Temporary, move to WaveLayerFactory once all layers have been added.
            if (data is BoundaryLineMapFeatureProvider boundaryLineMapFeatureProvider)
            {
                var groupLayer = new GroupLayer("Spatially Varying Wave Boundaries")
                {
                    LayersReadOnly = false,
                };

                var lineDataLayer = new VectorLayer("Wave Boundary")
                {
                    DataSource = boundaryLineMapFeatureProvider,
                    NameIsReadOnly = true,
                    FeatureEditor = new Feature2DEditor(model),
                    Style = new VectorStyle
                    {
                        Line = new Pen(Color.Blue, 3f),
                        GeometryType = typeof(ILineString)
                    },
                };

                groupLayer.Layers.Add(lineDataLayer);

                return groupLayer;
            }

            return null;
        }

        private ICoordinateSystem GetGridCoordinateSystem(object parent, IHasCoordinateSystem model, IEditableObject discreteGrid)
        {
            if (model != null)
            {
                return model.CoordinateSystem;
            }

            if (parent is WavmFileFunctionStore)
            {
                return null;
            }

            WaveModel ownerWaveModel = GetWaveModels?.Invoke()
                                                    .FirstOrDefault(w => w.GetAllItemsRecursive()
                                                                          .Contains(discreteGrid));
            return ownerWaveModel?.CoordinateSystem;
        }

        private static ILayer GetOutputLayer(WavmFileFunctionStore wavmFileFunctionStore, IWaveModel model)
        {
            string domainName = wavmFileFunctionStore.Path;
            var overrideDomainName = true;

            if (model != null)
            {
                IDataItem dataItem = model.GetDataItemByValue(wavmFileFunctionStore);
                string dataItemTag = dataItem.Tag;

                if (dataItemTag.StartsWith(WaveModel.WavmStoreDataItemTag))
                {
                    var paramsValue = new string(dataItemTag.Skip(WaveModel.WavmStoreDataItemTag.Length).ToArray());
                    domainName = string.Join(" ", paramsValue, "WAVM");
                    overrideDomainName = false;
                }
            }

            return WaveLayerFactory.CreateOutputLayer(domainName, overrideDomainName);
        }

        /// <summary>
        /// Determine if a layer can be created for the specified data
        /// </summary>
        /// <param name="data">Data to create a layer for</param>
        /// <param name="parentData">Parent data of the data</param>
        /// <returns>
        /// Whether a layer be created given the parameters.
        /// </returns>
        public bool CanCreateLayerFor(object data, object parentData)
        {
            return data is WaveModel
                   || data is WaveDomainData
                   || data is IDiscreteGridPointCoverage
                   || data is IEventedList<WaveObstacle>
                   || data is IEventedList<Feature2D>
                   || data is IEventedList<Feature2DPoint>
                   || data is IEventedList<WaveBoundaryCondition>
                   || data is WaveSnappedFeaturesGroupLayerData
                   || data is WavmFileFunctionStore
                   || data is CurvilinearCoverage
                   // TODO: Update once all mapcomponents are in place
                   || data is BoundaryLineMapFeatureProvider;
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            var model = data as WaveModel;
            if (model != null)
            {
                yield return new WaveSnappedFeaturesGroupLayerData(model);
                // TODO: Update once all mapcomponents are in place
                yield return new BoundaryLineMapFeatureProvider(model.BoundaryContainer,
                                                                new WaveBoundaryFactory(model.BoundaryContainer, 
                                                                                        new WaveBoundaryFactoryHelper()),
                                                                new GeometryFactory(model.BoundaryContainer));

                yield return model.BoundaryConditions;
                yield return model.Boundaries;
                yield return model.Sp2Boundaries;
                yield return model.Obstacles;
                yield return model.ObservationPoints;
                yield return model.ObservationCrossSections;
                foreach (WaveDomainData waveDomain in WaveDomainHelper.GetAllDomains(model.OuterDomain))
                {
                    yield return waveDomain;
                }

                foreach (
                    WavmFileFunctionStore wavmFunctionStore in
                    model.WavmFunctionStores.Where(fs => fs.Functions.Any() && !string.IsNullOrEmpty(fs.Path)))
                {
                    yield return wavmFunctionStore;
                }
            }

            var domain = data as WaveDomainData;
            if (domain != null)
            {
                yield return domain.Grid;
                yield return domain.Bathymetry;
            }

            var store = data as WavmFileFunctionStore;
            if (store != null)
            {
                WaveModel waveModel = GetWaveModels?.Invoke().FirstOrDefault(m => m.WavmFunctionStores.Contains(store));
                if (waveModel == null)
                {
                    yield return store.Grid;
                }

                foreach (IFunction coverage in store.Functions)
                {
                    yield return coverage;
                }
            }
        }

        public Func<IEnumerable<WaveModel>> GetWaveModels { get; set; }
    }
}