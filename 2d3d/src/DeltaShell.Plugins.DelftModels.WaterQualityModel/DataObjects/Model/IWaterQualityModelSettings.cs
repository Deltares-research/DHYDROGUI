using System;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model
{
    /// <summary>
    /// Interface for Water quality model settings
    /// </summary>
    public interface IWaterQualityModelSettings
    {
        /// <summary>
        /// The directory where all the temporary files are placed
        /// </summary>
        string WorkDirectory { get; }

        /// <summary>
        /// The start time of the his output
        /// </summary>
        DateTime HisStartTime { get; set; }

        /// <summary>
        /// The stop time of the his output
        /// </summary>
        DateTime HisStopTime { get; set; }

        /// <summary>
        /// The time step of the his output
        /// </summary>
        TimeSpan HisTimeStep { get; set; }

        /// <summary>
        /// The start time of the map output
        /// </summary>
        DateTime MapStartTime { get; set; }

        /// <summary>
        /// The stop time of the map output
        /// </summary>
        DateTime MapStopTime { get; set; }

        /// <summary>
        /// The time step of the map output
        /// </summary>
        TimeSpan MapTimeStep { get; set; }

        /// <summary>
        /// The start time of the balance output
        /// </summary>
        DateTime BalanceStartTime { get; set; }

        /// <summary>
        /// The stop time of the balance output
        /// </summary>
        DateTime BalanceStopTime { get; set; }

        /// <summary>
        /// The time step of the balance output
        /// </summary>
        TimeSpan BalanceTimeStep { get; set; }

        /// <summary>
        /// The numerical scheme
        /// </summary>
        NumericalScheme NumericalScheme { get; set; }

        /// <summary>
        /// Use flows and dispersion as specified (false) or only if the flow is not zero (true)
        /// </summary>
        bool NoDispersionIfFlowIsZero { get; set; }

        /// <summary>
        /// Use dispersion over open boundaries (false) or not (true)
        /// </summary>
        bool NoDispersionOverOpenBoundaries { get; set; }

        /// <summary>
        /// Use first order (true) or second order (false) approximation over open boundaries
        /// </summary>
        bool UseFirstOrder { get; set; }

        /// <summary>
        /// Whether or not to calculate mass balance
        /// </summary>
        bool Balance { get; set; }

        /// <summary>
        /// Report contribution of all individual processes (false) or have them 'lumped' into a single term (true)
        /// </summary>
        bool LumpProcesses { get; set; }

        /// <summary>
        /// Report contribution of all individual transports (false) or have them 'lumped' into a single term (true)
        /// </summary>
        bool LumpTransport { get; set; }

        /// <summary>
        /// Report contribution of all individual loads (false) or have them 'lumped' into a single term (true)
        /// </summary>
        bool LumpLoads { get; set; }

        /// <summary>
        /// Whether or not to suppress output for individual monitoring points (segments) and monitoring areas (leaving only the
        /// overall mass balance term)
        /// </summary>
        bool SuppressSpace { get; set; }

        /// <summary>
        /// Whether or not to suppress output for the monitoring time steps (leaving only the terms accumulated over time)
        /// </summary>
        bool SuppressTime { get; set; }

        /// <summary>
        /// The balance unit
        /// </summary>
        BalanceUnit BalanceUnit { get; set; }

        /// <summary>
        /// Whether or not to run with processes
        /// </summary>
        /// <remarks>
        /// This setting is only used during file based preprocessing
        /// </remarks>
        bool ProcessesActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Delwaq should correct water volumes when
        /// doing a 'wrap around' on the hydro dynamic data.
        /// </summary>
        /// <value>
        /// <c> true </c> if Delwaq should add/remove water volumes to keep concentrations continuous;
        /// otherwise, <c> false </c> where the water volumes are used as is (causing concentration
        /// discontinuities when wrapping around).
        /// </value>
        bool ClosureErrorCorrection { get; set; }

        /// <summary>
        /// Gets or sets the number of threads to be used by Delwaq.
        /// </summary>
        int NrOfThreads { get; set; }

        /// <summary>
        /// Gets or sets the water level threshold (m) to determine if cells are empty/dry
        /// (below this level) or containing water.
        /// </summary>
        double DryCellThreshold { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of iterations allowed for some calculation schemes.
        /// </summary>
        int IterationMaximum { get; set; }

        /// <summary>
        /// Gets or sets the convergence tolerance used in some calculation schemes.
        /// </summary>
        double Tolerance { get; set; }

        /// <summary>
        /// Gets or sets whether the report about the iteration should be written (true) or not.
        /// </summary>
        bool WriteIterationReport { get; set; }

        /// <summary>
        /// Gets or sets the working output directory.
        /// Output files will be placed here during a model run.
        /// </summary>
        /// <value>
        /// The working output directory.
        /// </value>
        string WorkingOutputDirectory { get; set; }
    }
}