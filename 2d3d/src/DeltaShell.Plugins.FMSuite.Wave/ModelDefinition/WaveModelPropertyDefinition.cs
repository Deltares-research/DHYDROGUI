using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Wave.ModelDefinition
{
    public class WaveModelPropertyDefinition : ModelPropertyDefinition {}

    /// <summary>
    /// <see cref="KnownWaveSections"/> defines the known and up to date
    /// sections in the .mdw file of a wave model.
    /// </summary>
    public static class KnownWaveSections
    {
        public const string GeneralSection = "General";
        public const string ProcessesSection = "Processes";
        public const string BoundarySection = "Boundary";
        public const string TimePointSection = "TimePoint";
        public const string OutputSection = "Output";
        public const string DomainSection = "Domain";
        public const string ObstacleSection = "Obstacle";
        public const string NumericsSection = "Numerics";

        public const string GuiOnlySection = "GUIOnly";
    }

    /// <summary>
    /// <see cref="KnownWaveProperties"/> defines the known and up to date
    /// properties in the .mdw file of a wave model.
    /// </summary>
    public static class KnownWaveProperties
    {
        public const string FlowFile = "FlowFile";
        public const string FlowMudFile = "FlowMudFile";

        public const string Time = "Time";
        public const string TimeStep = "TimeStep";
        public const string StartTime = "StartTime";
        public const string TimeScale = "TimeInterval";
        public const string StopTime = "StopTime";
        public const string MapWriteNetCDF = "MapWriteNetCDF";
        public const string WriteCOM = "WriteCOM";
        public const string WriteTable = "WriteTable";
        public const string Breaking = "Breaking";
        public const string Triads = "Triads";
        public const string Quadruplets = "Quadruplets";
        public const string Diffraction = "Diffraction";
        public const string BedFriction = "BedFriction";
        public const string BedFrictionCoef = "BedFricCoef";
        public const string TimeSeriesFile = "TSeriesFile";
        public const string ObstacleFile = "ObstacleFile";
        public const string ReferenceDate = "ReferenceDate";
        public const string SimulationMode = "SimMode";
        public const string WaveSetup = "WaveSetup";
        public const string InputTemplateFile = "INPUTTemplateFile";

        public const string LocationFile = "LocationFile";
        public const string CurveFile = "CurveFile";

        public const string WindSpeed = "WindSpeed";
        public const string WindDirection = "WindDir";
        public const string MeteoFile = "MeteoFile";

        public const string WaterLevel = "WaterLevel";
        public const string WaterVelocityX = "XVeloc";
        public const string WaterVelocityY = "YVeloc";
        public const string DirectionalSpaceType = "DirSpace";
        public const string NumberOfDirections = "NDir";
        public const string StartDirection = "StartDir";
        public const string EndDirection = "EndDir";
        public const string NumberOfFrequencies = "NFreq";
        public const string StartFrequency = "FreqMin";
        public const string EndFrequency = "FreqMax";

        public const string COMFile = "COMFile";
        public const string FlowBedLevelUsage = "FlowBedLevel";
        public const string FlowWaterLevelUsage = "FlowWaterLevel";
        public const string FlowVelocityUsage = "FlowVelocity";
        public const string FlowVelocityUsageType = "FlowVelocityType";
        public const string FlowWindUsage = "FlowWind";
        public const string MaxIter = "MaxIter";

        public const string MeteoQuantityField = "quantity1";
        public const string MeteoXComponentValue = "x_wind";
        public const string MeteoYComponentValue = "y_wind";

        public const string NetCdfSinglePrecision = "NetCDFSinglePrecision";
        public const string KeepINPUT = "KeepINPUT";

        #region Wave boundary conditions

        public const string Name = "Name";
        public const string Definition = "Definition";
        public const string StartCoordinateX = "StartCoordX";
        public const string EndCoordinateX = "EndCoordX";
        public const string StartCoordinateY = "StartCoordY";
        public const string EndCoordinateY = "EndCoordY";
        public const string SpectrumSpec = "SpectrumSpec";
        public const string Orientation = "Orientation";
        public const string DistanceDir = "DistanceDir";

        public const string ShapeType = "SpShapeType";
        public const string PeriodType = "PeriodType";
        public const string DirectionalSpreadingType = "DirSpreadType";
        public const string PeakEnhancementFactor = "PeakEnhanceFac";
        public const string GaussianSpreading = "GaussSpread";

        public const string Spectrum = "Spectrum";
        public const string WaveHeight = "WaveHeight";
        public const string Period = "Period";
        public const string Direction = "Direction";
        public const string DirectionalSpreadingValue = "DirSpreading";

        public const string CondSpecAtDist = "CondSpecAtDist";
        public const string OverallSpecFile = "OverallSpecfile";

        #endregion

        #region Wave domain

        public const string Grid = "Grid";
        public const string BedLevelGrid = "BedLevelGrid";
        public const string BedLevel = "BedLevel";

        #endregion
    }

    /// <summary>
    /// <see cref="KnownWaveObsSections"/> defines the known and up to date
    /// sections in the .obs file of a wave model.
    /// </summary>
    public static class KnownWaveObsSections
    {
        public const string ObstacleFileInformation = "ObstacleFileInformation";

    }

    /// <summary>
    /// <see cref="KnownWaveObsProperties"/> defines the known and up to date
    /// properties in the .obs file of a wave model.
    /// </summary>
    public static class KnownWaveObsProperties
    {
        public const string PolylineFile = "PolylineFile";
    }
}