using System;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    /// <summary>
    /// only 2.12
    /// Data found in the settings.dat file of the Sobek case 
    /// </summary>
    public class SobekCaseSettings
    {
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public TimeSpan TimeStep { get; set; }
        public TimeSpan OutPutTimeStep { get; set; }
        public bool PeriodFromEvent { get; set; }

        // Initial Conditions 
        public bool FromNetter { get; set; }
        public bool FromValuesSelected { get; set; }
        public bool FromRestart { get; set; }
        public bool InitialLevel { get; set; }
        public bool InitialDepth { get; set; }
        public bool InitialEmptyWells { get; set; }

        // ignore InitialDepth; is just !InitialLevel
        public double InitialFlowValue { get; set; }
        public double InitialLevelValue { get; set; }
        public double InitialDepthValue { get; set; }
       
        //data found in settings.dat.xls
        //simulation
        public int LateralLocation { get; set; }
        public bool NoNegativeQlatWhenThereIsNoWater { get; set; }
        public double MaxLoweringCrossAtCulvert { get; set; }
        // parameters
        // do not use them they obsolete!
        // flow Parameters
        public double GravityAcceleration { get; set; }
        public double Theta { get; set; }
        public double Rho { get; set; }
        public double RelaxationFactor { get; set; }
        public double CourantNumber { get; set; }
        public int MaxDegree { get; set; }
        public int MaxIterations { get; set; }
        public double DtMinimum { get; set; }
        public double EpsilonValueVolume { get; set; }
        public double EpsilonValueWaterDepth{ get; set; }
        public double StructureDynamicsFactor { get; set; }
        public double ThresholdValueFlooding { get; set; }
        public double ThresholdValueFloodingFLS { get; set; }
        public double MinimumLength { get; set; }
        public int AccurateVersusSpeed { get; set; }
        public double StructureInertiaDampingFactor { get; set; }
        public double MinimumSurfaceinNode { get; set; }
        public double MinimumSurfaceatStreet { get; set; }
        public double ExtraResistanceGeneralStructure { get; set; }
        public double AccelerationTermFactor { get; set; }
        public bool UseTimeStepReducerStructures { get; set; }
        public double? Iadvec1D { get; set; }
        public double? Limtyphu1D { get; set; }
        public double? Momdilution1D { get; set; }
        //ResultsGeneral
        public bool ActualValue { get; set; }
        public bool MeanValue{ get; set; }
        public bool MaximumValue { get; set; }
        //ResultsNodes
        public bool Freeboard { get; set; }
        public bool TotalArea { get; set; }
        public bool TotalWidth { get; set; }
        public bool Volume { get; set; }
        public bool WaterDepth { get; set; }
        public bool WaterLevelOnResultsNodes { get; set; }
        public bool LateralOnNodes { get; set; }
        //ResultsBranches
        public bool Chezy { get; set; }
        public bool Froude { get; set; }
        public bool RiverSubsectionParameters { get; set; }
        public bool WaterLevelSlope { get; set; }
        public bool Wind { get; set; }
        public bool DischargeOnResultsBranches { get; set; }
        public bool VelocityOnResultsBranches { get; set; }
        //ResultsStructures
        public bool CrestLevel { get; set; }
        public bool CrestWidth { get; set; }
        public bool GateLowerEdgeLevel { get; set; }
        public bool Head { get; set; }
        public bool OpeningsArea { get; set; }
        public bool PressureDifference { get; set; }
        public bool WaterlevelOnCrest { get; set; }
        public bool DischargeOnResultsStructures { get; set; }
        public bool VelocityOnResultsStructures { get; set; }
        public bool WaterLevelOnResultsStructures { get; set; }
        public bool CrestlevelOpeningsHeight { get; set; }
        //ResultsPumps
        public bool PumpResults { get; set; }
        //RiverOptions
        public double TransitionHeightSummerDike { get; set; }
        // SobekRe specific?
        public bool UseKsiForExtraResistance { get; set; }
        //Waterquality options
        public string MeasurementFile { get; set; }
        public bool? Fraction { get; set; }
        public int HistoryOutputInterval { get; set; }
        public int BalanceOutputInterval { get; set; }
        public bool HisPeriodFromSimulation { get; set; }
        public bool BalPeriodFromSimulation { get; set; }
        public bool PeriodFromFlow { get; set; }
        public bool ActiveProcess { get; set; }
        public bool UseOldQuantityResults { get; set; }
        public bool? LumpProcessesContributions { get; set; }
        public bool? LumpBoundaryContributions { get; set; }
        public bool? SumOfMonitoringAreas { get; set; }
        public bool? SuppressTimeDependentOutput { get; set; }
        public bool? LumpInternalTransport { get; set; }
        public int MapOutputInterval { get; set; }
        public bool MapPeriodFromSimulation { get; set; }
        public int OutputLocationsType { get; set; }
        public int? OutputHisVarType  { get; set; }
        public int? OutputHisMapType  { get; set; }
        public string SubstateFile { get; set; }
        public int? SubstateFileOption { get; set; }
        public bool UseTatcherHarlemanTimeLag { get; set; }
        public int TatcherHarlemanTimeLag { get; set; }
    }
}
