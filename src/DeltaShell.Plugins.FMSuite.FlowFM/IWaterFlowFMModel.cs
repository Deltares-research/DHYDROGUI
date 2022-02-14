using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    /// <summary>
    /// <see cref="IWaterFlowFMModel"/> describes the content of a FlowFM model.
    /// </summary>
    public interface IWaterFlowFMModel : ITimeDependentModel, 
                                         IModelWithNetwork,
                                         IEditableObject,
                                         IGridOperationApi,
                                         IHasCoordinateSystem,
                                         INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the <see cref="Grid"/> of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        UnstructuredGrid Grid { get; set; }

        /// <summary>
        /// Gets the <see cref="WaterFlowFMModelDefinition"/> of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        WaterFlowFMModelDefinition ModelDefinition { get; }

        bool DisableFlowNodeRenumbering { get; set; }

        IEventedList<ISedimentProperty> SedimentOverallProperties { get; }

        IEventedList<ISedimentFraction> SedimentFractions { get; }

        string MduFilePath { get; }
        IEventedList<Model1DBoundaryNodeData> BoundaryConditions1D { get; }

        IEventedList<Model1DLateralSourceData> LateralSourcesData { get; }

        /// <summary>
        /// Gets the boundaries of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEventedList<Feature2D> Boundaries { get; }

        /// <summary>
        /// Gets the set of <see cref="IBoundaryCondition"/> of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEnumerable<IBoundaryCondition> BoundaryConditions { get; }

        /// <summary>
        /// Gets the 1D/2D links of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEventedList<ILink1D2D> Links { get; }

        /// <summary>
        /// Gets the channel initial condition definitions of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEventedList<ChannelInitialConditionDefinition> ChannelInitialConditionDefinitions { get; }
        
        /// <summary>
        /// Gets the channel friction definitions of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEventedList<ChannelFrictionDefinition> ChannelFrictionDefinitions { get; }

        /// <summary>
        /// Gets the pipe friction definitions of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEventedList<PipeFrictionDefinition> PipeFrictionDefinitions { get; }

        /// <summary>
        /// Gets the roughness sections of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEventedList<RoughnessSection> RoughnessSections { get; }

        /// <summary>
        /// Gets the path to the output snapped features of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        string OutputSnappedFeaturesPath { get; }

        /// <summary>
        /// Gets whether to write the snapped features of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        bool WriteSnappedFeatures { get; }

        /// <summary>
        /// Gets the current snap version of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        int SnapVersion { get; }

        /// <summary>
        /// Gets the area of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        HydroArea Area { get; }

        /// <summary>
        /// Gets the <see cref="SourceAndSink"/> of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEventedList<SourceAndSink> SourcesAndSinks { get; }

        /// <summary>
        /// Gets the data item with the specified <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">The tag of the <see cref="IDataItem"/> to retrieve.</param>
        /// <returns>
        /// The corresponding <see cref="IDataItem"/>.
        /// </returns>
        /// <remarks>
        /// This is provided to the <see cref="WaterFlowFMModel"/> by the <see cref="ModelBase"/>
        /// </remarks>
        IDataItem GetDataItemByTag(string tag);

        /// <summary>
        /// Gets the output <see cref="FM1DFileFunctionStore"/> of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        FM1DFileFunctionStore Output1DFileStore { get; }

        /// <summary>
        /// Gets whether the output of this <see cref="IWaterFlowFMModel"/> is empty.
        /// </summary>
        bool OutputIsEmpty { get; }

        /// <summary>
        /// Gets the Bathymetry of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        UnstructuredGridCoverage Bathymetry { get; }

        /// <summary>
        /// Gets the initial water level of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        UnstructuredGridCellCoverage InitialWaterLevel { get; }

        /// <summary>
        /// Gets whether to use the initial salinity coverages.
        /// </summary>
        bool UseSalinity { get; }

        /// <summary>
        /// Gets the initial salinity of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        CoverageDepthLayersList InitialSalinity { get; }

        /// <summary>
        /// Gets the <see cref="HeatFluxModelType"/> of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        HeatFluxModelType HeatFluxModelType { get; }

        /// <summary>
        /// Gets the initial temperature of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        UnstructuredGridCellCoverage InitialTemperature { get; }

        /// <summary>
        /// Gets the roughness of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        UnstructuredGridFlowLinkCoverage Roughness { get; }

        /// <summary>
        /// Gets the viscosity of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        UnstructuredGridFlowLinkCoverage Viscosity { get; }

        /// <summary>
        /// Gets the diffusivity of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        UnstructuredGridFlowLinkCoverage Diffusivity { get; }

        /// <summary>
        /// Whether to use the infiltration of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        bool UseInfiltration { get; }

        /// <summary>
        /// Gets the infiltration of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        UnstructuredGridCellCoverage Infiltration { get; }

        /// <summary>
        /// Gets the initial tracers of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEventedList<UnstructuredGridCellCoverage> InitialTracers { get; }

        /// <summary>
        /// Whether to use morphology and sediments.
        /// </summary>
        bool UseMorSed { get; }

        /// <summary>
        /// Gets the initial fractions of this <see cref="IWaterFlowFMModel"/>
        /// </summary>
        IEventedList<UnstructuredGridCellCoverage> InitialFractions { get; }

        /// <summary>
        /// Gets the list of <see cref="BoundaryConditionSet"/> items of this
        /// <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEventedList<BoundaryConditionSet> BoundaryConditionSets { get; }

        /// <summary>
        /// Gets the pipes of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        IEventedList<Feature2D> Pipes { get; }

        /// <summary>
        /// Gets the <see cref="FMMapFileFunctionStore"/> of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        FMMapFileFunctionStore OutputMapFileStore { get; }

        /// <summary>
        /// Gets the <see cref="FMHisFileFunctionStore"/> of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        FMHisFileFunctionStore OutputHisFileStore { get; }

        /// <summary>
        /// Gets the <see cref="FMClassMapFileFunctionStore"/> of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        FMClassMapFileFunctionStore OutputClassMapFileStore { get; }

        /// <summary>
        /// Gets the <see cref="FouFileFunctionStore"/> of this <see cref="IWaterFlowFMModel"/>.
        /// </summary>
        FouFileFunctionStore OutputFouFileStore { get; }
    }
}