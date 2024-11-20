using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers
{
    /// <summary>
    /// <see cref="IFlowFMLayerInstanceCreator"/> describes the logic required
    /// to create the different <see cref="ILayer"/> objects used within the
    /// FlowFM GUI plugin.
    /// </summary>
    public interface IFlowFMLayerInstanceCreator
    {
        /// <summary>
        /// Creates a new model group layer.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The group layer describing the <paramref name="model"/>.</returns>
        ILayer CreateModelGroupLayer(IWaterFlowFMModel model);

        /// <summary>
        /// Creates a new "1D" group layer.
        /// </summary>
        /// <returns>The "1D" group layer.</returns>
        ILayer Create1DGroupLayer();

        /// <summary>
        /// Creates a new "2D" group layer.
        /// </summary>
        /// <returns>The "2D" group layer.</returns>
        ILayer Create2DGroupLayer();

        /// <summary>
        /// Creates a new "input" group layer.
        /// </summary>
        /// <returns>The "input" group layer.</returns>
        ILayer CreateInputGroupLayer();

        /// <summary>
        /// Creates a new "output" group layer.
        /// </summary>
        /// <returns>The "output" group layer.</returns>
        ILayer CreateOutputGroupLayer();

        /// <summary>
        /// Creates a new layer for 1D boundary node data.
        /// </summary>
        /// <param name="model">The model containing the boundary node data.</param>
        /// <returns>A layer describing the boundary node data.</returns>
        ILayer CreateBoundaryNodeDataLayer(IWaterFlowFMModel model);

        /// <summary>
        /// Creates a new layer for 1D lateral data.
        /// </summary>
        /// <param name="model">The model containing the lateral data.</param>
        /// <returns>A layer describing the lateral data.</returns>
        ILayer CreateLateralDataLayer(IWaterFlowFMModel model);

        /// <summary>
        /// Creates a new layer describing the boundaries.
        /// </summary>
        /// <param name="model">The model containing the boundary data.</param>
        /// <returns>A layer describing the boundaries.</returns>
        ILayer CreateBoundariesLayer(IWaterFlowFMModel model);

        /// <summary>
        /// Creates a new layer describing the boundary condition sets.
        /// </summary>
        /// <param name="model">The model containing the boundary condition sets.</param>
        /// <returns>A layer describing the boundary condition sets.</returns>
        ILayer CreateBoundaryConditionSetsLayer(IWaterFlowFMModel model);

        /// <summary>
        /// Creates a new layer describing the pipes.
        /// </summary>
        /// <param name="model">The model containing the pipes.</param>
        /// <returns>A layer describing the pipes.</returns>
        ILayer CreatePipesLayer(IWaterFlowFMModel model);

        /// <summary>
        /// Creates a new layer describing the 1D2D links.
        /// </summary>
        /// <param name="model">The model containing the 1D2D links.</param>
        /// <returns>A layer describing the 1D2D links.</returns>
        ILayer CreateLinks1D2DLayer(IWaterFlowFMModel model);

        /// <summary>
        /// Creates a new layer describing the 1D2D links.
        /// </summary>
        /// <param name="data">The 1D2D links.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>A layer describing the 1D2D links.</returns>
        ILayer CreateLinks1D2DLayer(IList<ILink1D2D> data, ICoordinateSystem coordinateSystem);

        /// <summary>
        /// Creates a new group layer which will contain the individual component layers
        /// of the 1D map function store.
        /// </summary>
        /// <returns>
        /// A group layer which will contain the individual component layers of the 1D
        /// map file.
        /// </returns>
        ILayer CreateMapFileFunctionStore1DLayer();

        /// <summary>
        /// Creates a new group layer which will contain the individual component layers
        /// of the 2D map function store.
        /// </summary>
        /// <returns>
        /// A group layer which will contain the individual component layers of the 2D
        /// map file.
        /// </returns>
        ILayer CreateMapFileFunctionStore2DLayer();

        /// <summary>
        /// Creates a new group layer which will contain the individual component layers
        /// of the history function store.
        /// </summary>
        /// <returns>
        /// A group layer which will contain the individual component layers of the
        /// history function store.
        /// </returns>
        ILayer CreateHisFileFunctionStoreLayer();

        /// <summary>
        /// Creates a new group layer which will contain the individual component layers
        /// of the class map function store.
        /// </summary>
        /// <returns>
        /// A group layer which will contain the individual component layers of the
        /// class map function store.
        /// </returns>
        ILayer CreateClassMapFileFunctionStoreLayer();

        /// <summary>
        /// Creates a new group layer which will contain the individual component layers
        /// of the fou file function store.
        /// </summary>
        /// <returns>
        /// A group layer which will contain the individual component layers of the
        /// fou file function store.
        /// </returns>
        ILayer CreateFouFileFunctionStoreLayer();

        /// <summary>
        /// Creates a new function grouping layer describing the provided
        /// <paramref name="grouping"/>.
        /// </summary>
        /// <param name="grouping">The grouping for which to create the layer</param>
        /// <returns>
        /// The function grouping layer.
        /// </returns>
        ILayer CreateFunctionGroupingLayer(IGrouping<string, IFunction> grouping);

        /// <summary>
        /// Creates a new layer describing the provided <paramref name="netFile"/>.
        /// </summary>
        /// <param name="netFile">The net file described by the created layer.</param>
        /// <returns>
        /// A layer describing the provided <paramref name="netFile"/>.
        /// </returns>
        ILayer CreateImportedFMNetFileLayer(ImportedFMNetFile netFile);

        /// <summary>
        /// Creates a new levee breach width coverage layer describing the provided
        /// levee breach width <paramref name="coverage"/>.
        /// </summary>
        /// <param name="coverage">The levee breach width coverage.</param>
        /// <returns>
        /// A levee breach width coverage layer describing the provided
        /// <paramref name="coverage"/>.
        /// </returns>
        ILayer CreateLeveeBreachWidthCoverageLayer(FeatureCoverage coverage);

        /// <summary>
        /// Creates a new (1D) friction group layer.
        /// </summary>
        /// <returns>
        /// A (1D) friction group layer.
        /// </returns>
        ILayer CreateFrictionGroupLayer();

        /// <summary>
        /// Creates a new (1D) initial conditions group layer.
        /// </summary>
        /// <returns>
        /// A (1D) initial conditions group layer
        /// </returns>
        ILayer CreateInitialConditionsGroupLayer();

        /// <summary>
        /// Creates a new definitions layer describing the provided
        /// <paramref name="definitions"/> of type <typeparamref name="TDefinition"/>.
        /// </summary>
        /// <typeparam name="TDefinition">The type of definitions.</typeparam>
        /// <param name="layerName">The name of the new layer.</param>
        /// <param name="definitions">The definitions visualized in this new layer</param>
        /// <param name="network">The network of the definitions.</param>
        /// <returns>
        /// A definitions layer describing the provided <paramref name="definitions"/>.
        /// </returns>
        ILayer CreateDefinitionsLayer<TDefinition>(string layerName,
                                                   IEventedList<TDefinition> definitions,
                                                   IHydroNetwork network)
            where TDefinition : IFeature;

        /// <summary>
        /// Creates a new group layer for the output snapped features.
        /// </summary>
        /// <returns>
        /// A group layer for the output snapped features.
        /// </returns>
        ILayer CreateOutputSnappedFeatureGroupLayer();

        /// <summary>
        /// Creates a new snapped feature layer.
        /// </summary>
        /// <param name="layerName">The name of the new layer.</param>
        /// <param name="featureDataPath">The path to the source file.</param>
        /// <param name="model">The model to which this feature belongs.</param>
        /// <returns>A layer containing the provided snapped feature.</returns>
        ILayer CreateOutputSnappedFeatureLayer(string layerName, 
                                               string featureDataPath, 
                                               IWaterFlowFMModel model);

        /// <summary>
        /// Creates a new group layer for the estimated-snapped features.
        /// </summary>
        /// <returns>The group layer for the estimated-snapped features</returns>
        ILayer CreateEstimatedSnappedFeatureGroupLayer();

        /// <summary>
        /// Creates a new estimated-snapped feature layer for the given <paramref name="featureType"/>.
        /// </summary>
        /// <param name="model">The model to which the estimated feature belongs.</param>
        /// <param name="featureType">The type of feature to create a layer for.</param>
        /// <returns>An estimated-snapped feature layer.</returns>
        ILayer CreateEstimatedSnappedFeatureLayer(IWaterFlowFMModel model, 
                                                  EstimatedSnappedFeatureType featureType);
    }
}