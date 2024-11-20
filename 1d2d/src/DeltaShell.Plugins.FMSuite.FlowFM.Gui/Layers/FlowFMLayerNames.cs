using DeltaShell.NGHS.IO.FileWriters.Roughness;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers
{
    /// <summary>
    /// <see cref="FlowFMLayerNames"/> defines the names of the different FlowFM layers.
    /// </summary>
    public static class FlowFMLayerNames
    {
        /// <summary>
        /// The boundaries layer name.
        /// </summary>
        public const string BoundariesLayerName = "Boundaries";

        /// <summary>
        /// The boundary conditions layer name.
        /// </summary>
        public const string BoundaryConditionsLayerName = "Boundary Conditions";

        /// <summary>
        /// The sources and sinks layer name.
        /// </summary>
        public const string SourcesAndSinksLayerName = "Sources and Sinks";

        /// <summary>
        /// The output snapped features layer name.
        /// </summary>
        public const string OutputSnappedFeaturesLayerName = "Output Snapped Features";

        /// <summary>
        /// Grid snapped features layer name.
        /// </summary>
        public const string EstimatedSnappedFeaturesLayerName = "Estimated Grid-snapped Features";

        /// <summary>
        /// The 1D/2D links layer name.
        /// </summary>
        public const string Links1D2DLayerName = "1D/2D Links";

        /// <summary>
        /// The boundary data 1D layer name.
        /// </summary>
        public const string BoundaryData1DLayerName = "Boundary Data";

        /// <summary>
        /// The lateral data 1D layer name.
        /// </summary>
        public const string LateralData1DLayerName = "Lateral Data";

        /// <summary>
        /// The roughness data 1D layer name.
        /// </summary>
        public const string Friction1DGroupLayerName = "Roughness Data";

        /// <summary>
        /// Gets the channel friction definitions layer name.
        /// </summary>
        public static string ChannelFrictionDefinitionsLayerName =>
            Properties.Resources.ChannelFrictionDefinitions_Name;

        /// <summary>
        /// Gets the pipe friction definitions layer name.
        /// </summary>
        public static string PipeFrictionDefinitionsLayerName =>
            Properties.Resources.PipeFrictionDefinitions_Name;

        /// <summary>
        /// The initial conditions 1D layer name.
        /// </summary>
        public const string InitialConditions1DGroupLayerName = "Initial Conditions";

        /// <summary>
        /// Gets the channel initial condition definitions layer name.
        /// </summary>
        public static string ChannelInitialConditionDefinitionsLayerName =>
            RoughnessDataRegion.SectionId.DefaultValue;

        /// <summary>
        /// The 1D group layer name.
        /// </summary>
        public const string GroupLayer1DName = "1D";

        /// <summary>
        /// The 2D group layer name.
        /// </summary>
        public const string GroupLayer2DName = "2D";

        /// <summary>
        /// The input group layer name.
        /// </summary>
        public const string InputGroupLayerName = "Input";

        /// <summary>
        /// The output group layer name.
        /// </summary>
        public const string OutputGroupLayerName = "Output";

        /// <summary>
        /// The Map File 1D group layer name.
        /// </summary>
        public const string MapFile1DGroupLayerName = "Map File 1D";

        /// <summary>
        /// The Map File 2D group layer name.
        /// </summary>
        public const string MapFile2DGroupLayerName = "Map File 2D";

        /// <summary>
        /// The Class Map File group layer name.
        /// </summary>
        public const string ClassMapFileGroupLayerName = "Class Map File";

        /// <summary>
        /// The History File group layer name.
        /// </summary>
        public const string HistoryFileGroupLayerName = "His File";

        /// <summary>
        /// The Fou File group layer name.
        /// </summary>
        public const string FouFileGroupLayerName = "Fou File";

        /// <summary>
        /// The estimated snapped observation points layer name.
        /// </summary>
        public const string EstimatedSnappedObservationPoints = "Observation Points";
            
        /// <summary>
        /// The estimated snapped thin dams layer name.
        /// </summary>
        public const string EstimatedSnappedThinDams = "Thin Dams";

        /// <summary>
        /// The estimated snapped fixed weirs layer name.
        /// </summary>
        public const string EstimatedSnappedFixedWeirs = "Fixed Weirs";

        /// <summary>
        /// The estimated snapped levee breaches layer name.
        /// </summary>
        public const string EstimatedSnappedLeveeBreaches = "Levee Breaches";

        /// <summary>
        /// The estimated snapped roof areas layer name.
        /// </summary>
        public const string EstimatedSnappedRoofAreas = "Roof Areas";

        /// <summary>
        /// The estimated snapped dry points layer name.
        /// </summary>
        public const string EstimatedSnappedDryPoints = "Dry Points";

        /// <summary>
        /// The estimated snapped dry areas layer name.
        /// </summary>
        public const string EstimatedSnappedDryAreas = "Dry Areas";

        /// <summary>
        /// The estimated snapped enclosures layer name.
        /// </summary>
        public const string EstimatedSnappedEnclosures = "Enclosures";

        /// <summary>
        /// The estimated snapped pumps layer name.
        /// </summary>
        public const string EstimatedSnappedPumps = "Pumps";

        /// <summary>
        /// The estimated snapped weirs layer name.
        /// </summary>
        public const string EstimatedSnappedWeirs = "Weirs";

        /// <summary>
        /// The estimated snapped gates layer name.
        /// </summary>
        public const string EstimatedSnappedGates = "Gates";

        /// <summary>
        /// The estimated snapped observation cross sections layer name.
        /// </summary>
        public const string EstimatedSnappedObservationCrossSections = "Observation Cross Sections";

        /// <summary>
        /// The estimated snapped embankments layer name.
        /// </summary>
        public const string EstimatedSnappedEmbankments = "Embankments";

        /// <summary>
        /// The estimated snapped sources and sinks layer name.
        /// </summary>
        public const string EstimatedSnappedSourcesAndSinks = "Sources and Sinks";

        /// <summary>
        /// The estimated snapped boundaries layer name.
        /// </summary>
        public const string EstimatedSnappedBoundaries = "Boundaries";

        /// <summary>
        /// The estimated snapped water level boundary points layer name.
        /// </summary>
        public const string EstimatedSnappedBoundariesWaterLevel = "Water Level Boundary Points";

        /// <summary>
        /// The estimated snapped discharge / velocity boundary points layer name.
        /// </summary>
        public const string EstimatedSnappedBoundariesVelocity = "Discharge / Velocity Boundary Points";
    }
}