namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public interface IWaqModelApi
    {
        /// <summary>
        /// Set process definition file
        /// </summary>
        /// <param name="mode">Subset of the process lib to use</param>
        /// <param name="processDefinitionFile">Path to the process definition file</param>
        bool SetWQProcessDefinition(string mode, string processDefinitionFile);

        /// <summary>
        /// Enables model logging
        /// </summary>
        bool LoggingEnabled { get; set; }

        /// <summary>
        /// Sets the time parameters for the run
        /// </summary>
        /// <param name="startTime">Start time of the simulation (s)</param>
        /// <param name="endDate">End time of the simulation (s)</param>
        /// <param name="timeStep">Time step size (s)</param>
        bool SetWQSimulationTimes(int startTime, int endDate, int timeStep);
        
        /// <summary>
        /// Sets the reference date to use for this run
        /// </summary>
        /// <param name="year_in">Reference date year</param>
        /// <param name="month_in">Reference date month</param>
        /// <param name="day_in">Reference date day</param>
        /// <param name="hour_in">Reference date hours</param>
        /// <param name="minute_in">Reference date minutes</param>
        /// <param name="second_in">Reference date seconds</param>
        bool SetWQReferenceDateTime(int year_in, int month_in, int day_in, int hour_in, int minute_in, int second_in);

        /// <summary>
        /// Set the output timers for MAP, HIS and balance
        /// </summary>
        /// <param name="type">Type of timer to set (1 = balance, 2 = MAP, 3 = HIS)</param>
        /// <param name="startTime">Start time to use</param>
        /// <param name="stopTime">End time to use</param>
        /// <param name="timeStep">Time step size (s)</param>
        bool SetWQOutputTimers(int type, int startTime, int stopTime, int timeStep);

        /// <summary>
        /// Set initial value for variable
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">Initial value</param>
        bool SetWQCurrentValueScalarInit(string name, double value);

        bool SetWQCurrentValueScalarRun(string name, double value);

        int GetWQItemIndex(int type, string name);

        int GetWQLocationCount(int type);

        /// <summary>
        /// Get current values for a substance
        /// </summary>
        /// <param name="name">Name of the substance</param>
        /// <param name="value">Array that will contain the values 
        /// (must be as large as the number of segments)</param>
        double[] GetWQCurrentValue(string name, int numberOfElements);

        /// <summary>
        /// Set integration options for Delwaq solver (Block 2)
        /// </summary>
        /// <param name="method"> Integration method number
        ///  1 - 1st order upwind in time and space
        ///  2 - Modified 2nd order Runge Kutta in time, 1st order upwind in space
        ///  3 - 2nd order Lax Wendroff
        ///  4 - Alternating direction implicit
        ///  5 - 2nd order Flux Corrected Transport (Boris and Book)
        ///  6 - Implicit steady state, direct method, 1st order upwind
        ///  7 - Implicit steady state, direct method 2nd order
        ///  8 - Iterative steady state, backward differences
        ///  9 - Iterative steady state, central differences
        /// 10 - Implicit, direct method, 1st order upwind
        /// 11 - Horizontally method 1, vertically implicit 2nd order
        /// 12 - Horizontally method 5, vertically implicit 2nd order
        /// 13 - Horizontally method 1, vertically implicit 1st order
        /// 14 - Horizontally method 5, vertically implicit 1st order
        /// 15 - Implicit iterative Method, 1st order upwind in space and time
        /// 16 - Implicit Iterative Method, 1st order upwind in horizontal and time, 2nd order vertically
        /// 17 - Iterative steady state, 1st order upwind in space
        /// 18 - Iterative steady state, 1st order upwind horizontally, 2nd order central vertically
        /// 19 - ADI horizontally, implicit 1st order upwind vertically
        /// 20 - ADI horizontally, implicit 2nd order central vertically
        /// 21 - Self adapting Theta Method, implicit vertically, FCT (Zalezac)
        /// 22 - Self adapting Theta Method, implicit vertically, FCT (Boris and Book)
        /// </param>
        /// <param name="dispZeroFlow">DELWAQ uses flows and dispersion as specified (false) or 
        /// it uses dispersions only if the flow is not zero (true)</param>
        /// <param name="dispBound">Used dispersion over open boundaries</param>
        /// <param name="firstOrder">Use first order(true) or second order (false) approximation over open boundaries</param>
        /// <param name="forester">Forester filter (in the vertical) is available for numerical schemes 3, 11, 12, 16 and 19.</param>
        /// <param name="anticreep">Anticreep filter can only be used for numerical schemes 19 and 20.</param>
        bool SetWQIntegrationOptions(int method, bool dispZeroFlow, bool dispBound, bool firstOrder, bool forester, bool anticreep);

        /// <summary>
        /// Set the options for generating output balance
        /// </summary>
        /// <param name="type">Type of output 
        /// 0 - No output balance
        /// 1 - Not to be used
        /// 2 - Mass balance
        /// 3 - Extended mass balance</param>
        /// <param name="lumpProcesses">Report contribution of all individual processes (false) or
        /// have them 'lumped' into a single term (true)</param>
        /// <param name="lumpLoads">Report contribution of all individual loads (false) or
        /// have them 'lumped' into a single term (true)</param>
        /// <param name="lumpTransport">Report contribution of all individual transports (false) or
        /// have them 'lumped' into a single term (true)</param>
        /// <param name="suppressSpace">Suppress output for individual monitoring points (segments) and 
        /// monitoring areas (leaving only the overall mass balance term)</param>
        /// <param name="suppressTime">Suppress output for the monitoring time steps (leaving only the 
        /// terms accumulated over time)</param>
        /// <param name="unitType">Unit type for mass balance
        /// 1 - The terms can simply be the total mass (unit: g)
        /// 2 - The terms can be divided by the total surface area (unit: g/m2)
        /// 3 - The terms can be divided by the total volume (unit: g/m3)</param>
        bool SetWQBalanceOutputOptions(int type, bool lumpProcesses, bool lumpLoads, bool lumpTransport, bool suppressSpace, bool suppressTime, int unitType);

        /// <summary>
        /// Defines the segment schematization
        /// </summary>
        /// <param name="numberSegments">The number of used segments (without boundaries)</param>
        /// <param name="pointerTable"> Defines exchange-surfaces between the cells
        /// <example> 
        ///       Segments: -1 | 1 | 2 | 3 | -2 
        ///         
        ///       four segment numbers per exchange:
        /// 
        ///       from, to, from-1, to+1
        ///         -1,  1,      0,    2
        ///          1,  2,     -1,    3
        ///          2,  3,      1,   -2
        ///          3, -2,      2,    0
        /// 
        ///      Where negative numbers are boundaries and 0 is not used
        /// </example> 
        /// </param>
        /// <param name="numberExchanges">Array with 4 elements : number of exchanges in X, Y, Z, Z bottom layers</param>
        /// <returns>if definition was successfully set</returns>
        bool DefineWQSchematisation(int numberSegments, int[] pointerTable, int[] numberExchanges);

        /// <summary>
        /// Set global dispersion per exchange direction
        /// </summary>
        /// <param name="dispc">Default global dispersion per exchange direction (X, Y, Z)</param>
        /// <param name="length">List of half segment length (two per exchange (from - to)) </param>
        bool DefineWQDispersion(double[] dispc, double[] length);

        /// <summary>
        /// Configure water quality processes and substances.
        /// </summary>
        /// <param name="substance">List of substance names</param>
        /// <param name="numberSubstances">Number of substances</param>
        /// <param name="numberTransported">Number of active substances (<= <param name="numberSubstances"/>)</param>
        /// <param name="processParameter">Process parameters to use</param>
        /// <param name="numberParameters">Number of <param name="processParameter"/> </param>
        /// <param name="process">List of process names</param>
        /// <param name="numberProcesses">Number of processes</param>
        bool DefineWQProcesses(string[] substance, int numberSubstances, int numberTransported,
                               string[] processParameter, int numberParameters, string[] process,
                               int numberProcesses);
        /// <summary>
        /// Set the boundary locations
        /// </summary>
        /// <param name="cell">Boundary location segment numbers</param>
        /// <param name="numberLocations">Number of boundary locations</param>
        bool DefineWQDischargeLocations(int[] cell, int numberLocations);
        
        /// <summary>
        /// Set the monitoring locations for HIS output
        /// </summary>
        /// <param name="cell">Monitoring location segment numbers</param>
        /// <param name="name">Monitoring location names</param>
        /// <param name="numberLocations">Number of monitoring location segments</param>
        bool DefineWQMonitoringLocations(int[] cell, string[] name, int numberLocations);

        /// <summary>
        /// Set initial volume for all segments
        /// </summary>
        /// <param name="volume">Volume (m3) per segment</param>
        bool SetWQInitialVolume(double[] volume);

        /// <summary>
        /// Set flows on exchanges
        /// </summary>
        /// <param name="volume">Volume per segment</param>
        /// <param name="area">Area per exchange</param>
        /// <param name="flow">Flow per exchange</param>
        bool SetWQFlowData(double[] volume, double[] area, double[] flow);

        /// <summary>
        /// Correct the mass for all substances via new volumes and horizontal surface areas
        /// Note: use once after ModelInitialize or ModelInitialize_by_Id
        /// Note: the initial volume and the horizontal surface area should NOT be zero
        /// </summary>
        /// <param name="volume">Volume per segment</param>
        /// <param name="surf">Surface per segment</param>
        bool CorrectWQVolumeSurface(double[] volume, double[] surf);

        /// <summary>
        /// Set the waste load values.
        /// </summary>
        /// <param name="idx">Index of the location specified in <see cref="DefineWQDischargeLocations"/></param>
        /// <param name="value">First value is discharge rate (m3/s), 
        /// other values are concentration of substances</param>
        bool SetWQWasteLoadValues(int idx, double[] value);

        /// <summary>
        /// Set the boundary conditions.
        /// </summary>
        /// <param name="idx">Boundary segment number</param>
        /// <param name="value">Values are concentrations of active substances</param>
        bool SetWQBoundaryConditions(int idx, double[] value);
        
        /// <summary>
        /// Lets Delwaq compute 1 time step
        /// </summary>
        bool WaqPerformTimeStep();

        /// <summary>
        /// Initialize model
        /// </summary>
        bool WaqInitialize();

        /// <summary>
        /// Initialize model with Delwaq 1 output
        /// </summary>
        /// <param name="name">Name of the output files</param>
        bool WaqInitialize_By_Id(string name);

        /// <summary>
        /// Free delwaq resources
        /// </summary>
        bool WaqFinalize();

        /// <summary>
        /// Gets the current time (step)
        /// </summary>
        double WaqGetCurrentTime();

        void LogMessages();

        /// <summary>
        /// Set the current value of a substance or process parameter: this function provides
        /// a general interface to the state variables and computational parameters
        /// </summary>
        /// <param name="parameterName">Name of the parameter to set</param>
        /// <param name="parameterType">Type of parameter to set</param>
        /// <param name="value">Values to use</param>
        bool SetWQValuesGeneral(string parameterName, DelwaqItem parameterType, double[] value);

        /// <summary>
        /// Writes a restart file for the current time step
        /// </summary>
        void WriteRestart();
    }
}
