using System;
using System.IO;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model
{
    /// <summary>
    /// Water quality model settings
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class WaterQualityModelSettings : Unique<long>, ICloneable, IWaterQualityModelSettings
    {
        /// <summary>
        /// Creates water quality model settings
        /// </summary>
        public WaterQualityModelSettings()
        {
            WorkingDirectoryPathFuncWithModelName =
                () => Path.Combine(DefaultModelSettings.DefaultDeltaShellWorkingDirectory, "Water_Quality");
            HisStartTime = DateTime.Now.Date;
            HisStopTime = HisStartTime.AddHours(24);
            HisTimeStep = new TimeSpan(0, 1, 0, 0);
            MapStartTime = DateTime.Now.Date;
            MapStopTime = MapStartTime.AddHours(24);
            MapTimeStep = new TimeSpan(0, 1, 0, 0);
            BalanceStartTime = DateTime.Now.Date;
            BalanceStopTime = BalanceStartTime.AddHours(24);
            BalanceTimeStep = new TimeSpan(0, 1, 0, 0);
            BalanceUnit = BalanceUnit.Gram;
            NumericalScheme = NumericalScheme.Scheme15;
            NoDispersionIfFlowIsZero = true;
            NoDispersionOverOpenBoundaries = true;
            UseFirstOrder = false;
            LumpProcesses = true;
            LumpTransport = true;
            LumpLoads = true;
            SuppressSpace = true;
            SuppressTime = true;
            NoBalanceMonitoringPoints = true;
            NoBalanceMonitoringAreas = true;
            NoBalanceMonitoringModelWide = false;
            ProcessesActive = true;
            MonitoringOutputLevel = MonitoringOutputLevel.None;
            CorrectForEvaporation = true;
            ClosureErrorCorrection = true;
            NrOfThreads = 2;
            DryCellThreshold = 0.001;
            IterationMaximum = 100;
            Tolerance = 1e-7;
            WriteIterationReport = false;
        }

        /// <summary>
        /// Function for retrieving the latest status of the DeltaShell framework
        /// working directory and model name from the model
        /// </summary>
        public Func<string> WorkingDirectoryPathFuncWithModelName { get; set; }

        /// <summary>
        /// Whether or not to use a forester filter
        /// </summary>
        /// <remarks>
        /// A forester filter (in the vertical) is available for the numerical schemes 3, 11, 12, 16 and 19
        /// </remarks>
        public bool UseForesterFilter { get; set; }

        /// <summary>
        /// Whether or not to use an anti creep filter
        /// </summary>
        /// <remarks>
        /// An anti creep filter can only be used for numerical schemes 19 and 20
        /// </remarks>
        public bool UseAnticreepFilter { get; set; }

        /// <summary>
        /// Whether or not to skip writing balance output for monitoring points.
        /// </summary>
        public bool NoBalanceMonitoringPoints { get; set; }

        /// <summary>
        /// Whether or not to skip writing balance output for monitoring areas.
        /// </summary>
        public bool NoBalanceMonitoringAreas { get; set; }

        /// <summary>
        /// Whether or not to skip writing model wide balance output.
        /// </summary>
        public bool NoBalanceMonitoringModelWide { get; set; }

        /// <summary>
        /// The monitoring output level
        /// </summary>
        public MonitoringOutputLevel MonitoringOutputLevel { get; set; }

        /// <summary>
        /// Whether or not to perform corrections for evaporation nodes
        /// </summary>
        public bool CorrectForEvaporation { get; set; }

        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        /// <value>
        /// The persistent output directory.
        /// </value>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// The directory where all the temporary files are placed
        /// </summary>
        public string WorkDirectory => WorkingDirectoryPathFuncWithModelName();

        /// <summary>
        /// The start time of the his output
        /// </summary>
        public DateTime HisStartTime { get; set; }

        /// <summary>
        /// The stop time of the his output
        /// </summary>
        public DateTime HisStopTime { get; set; }

        /// <summary>
        /// The time step of the his output
        /// </summary>
        public TimeSpan HisTimeStep { get; set; }

        /// <summary>
        /// The start time of the map output
        /// </summary>
        public DateTime MapStartTime { get; set; }

        /// <summary>
        /// The stop time of the map output
        /// </summary>
        public DateTime MapStopTime { get; set; }

        /// <summary>
        /// The time step of the map output
        /// </summary>
        public TimeSpan MapTimeStep { get; set; }

        /// <summary>
        /// The start time of the balance output
        /// </summary>
        public DateTime BalanceStartTime { get; set; }

        /// <summary>
        /// The stop time of the balance output
        /// </summary>
        public DateTime BalanceStopTime { get; set; }

        /// <summary>
        /// The time step of the balance output
        /// </summary>
        public TimeSpan BalanceTimeStep { get; set; }

        /// <summary>
        /// The numerical scheme
        /// </summary>
        public NumericalScheme NumericalScheme { get; set; }

        /// <summary>
        /// Use flows and dispersion as specified (false) or only if the flow is not zero (true)
        /// </summary>
        public bool NoDispersionIfFlowIsZero { get; set; }

        /// <summary>
        /// Use dispersion over open boundaries (false) or not (true)
        /// </summary>
        public bool NoDispersionOverOpenBoundaries { get; set; }

        /// <summary>
        /// Use first order (true) or second order (false) approximation over open boundaries
        /// </summary>
        public bool UseFirstOrder { get; set; }

        /// <summary>
        /// Whether or not to calculate mass balance
        /// </summary>
        public bool Balance { get; set; }

        /// <summary>
        /// Report contribution of all individual processes (false) or have them 'lumped' into a single term (true)
        /// </summary>
        public bool LumpProcesses { get; set; }

        /// <summary>
        /// Report contribution of all individual transports (false) or have them 'lumped' into a single term (true)
        /// </summary>
        public bool LumpTransport { get; set; }

        /// <summary>
        /// Report contribution of all individual loads (false) or have them 'lumped' into a single term (true)
        /// </summary>
        public bool LumpLoads { get; set; }

        /// <summary>
        /// Whether or not to suppress output for individual monitoring points (segments) and monitoring areas (leaving only the
        /// overall mass balance term)
        /// </summary>
        public bool SuppressSpace { get; set; }

        /// <summary>
        /// Whether or not to suppress output for the monitoring time steps (leaving only the terms accumulated over time)
        /// </summary>
        public bool SuppressTime { get; set; }

        /// <summary>
        /// The balance unit
        /// </summary>
        public BalanceUnit BalanceUnit { get; set; }

        /// <summary>
        /// Whether or not to run with processes
        /// </summary>
        /// <remarks>
        /// This setting is only used during file based preprocessing
        /// </remarks>
        public bool ProcessesActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Delwaq should correct water volumes when
        /// doing a 'wrap around' on the hydro dynamic data.
        /// </summary>
        /// <value>
        /// <c> true </c> if Delwaq should add/remove water volumes to keep concentrations continuous;
        /// otherwise, <c> false </c> where the water volumes are used as is (causing concentration
        /// discontinuities when wrapping around).
        /// </value>
        public bool ClosureErrorCorrection { get; set; }

        /// <summary>
        /// Gets or sets the number of threads to be used by Delwaq.
        /// </summary>
        public int NrOfThreads { get; set; }

        /// <summary>
        /// Gets or sets the water level threshold (m) to determine if cells are empty/dry
        /// (below this level) or containing water.
        /// </summary>
        public double DryCellThreshold { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of iterations allowed for some calculation schemes.
        /// </summary>
        public int IterationMaximum { get; set; }

        /// <summary>
        /// Gets or sets the convergence tolerance used in some calculation schemes.
        /// </summary>
        public double Tolerance { get; set; }

        /// <summary>
        /// Gets or sets whether the report about the iteration should be written (true) or not.
        /// </summary>
        public bool WriteIterationReport { get; set; }

        /// <summary>
        /// Gets or sets the working output directory.
        /// Output files will be placed here during a model run.
        /// </summary>
        /// <value>
        /// The working output directory.
        /// </value>
        public string WorkingOutputDirectory { get; set; }

        public object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }
    }
}