namespace DeltaShell.NGHS.IO.Helpers
{
    public static class ModelDefinitionsRegion
    {
        public const string FilesIniHeader = "Files";
        public static readonly ConfigurationSetting NetworkFile = new ConfigurationSetting(key: "networkFile", description: "#Name and location of the network file");
        public static readonly ConfigurationSetting NetworkUGridFile = new ConfigurationSetting(key: "networkUgridFile", description: "#Name and location of the network file in ugrid format");
        public static readonly ConfigurationSetting CrossSectionLocationsFile = new ConfigurationSetting(key: "crossLocFile", description: "Name and location of the file containing the locations of the cross sections");
        public static readonly ConfigurationSetting CrossSectionDefinitionsFile = new ConfigurationSetting(key: "crossDefFile", description: "Name and location of the file containing the definitions of the cross sections");
        public static readonly ConfigurationSetting StructuresFile = new ConfigurationSetting(key: "structureFile", description: "Name and location of the structure file");
        public static readonly ConfigurationSetting ObservationPointsFile = new ConfigurationSetting(key: "obsPointsFile", description: "Name and location of the observation points file");
        public static readonly ConfigurationSetting RoughnessFile = new ConfigurationSetting(key: "roughnessFile", description: "Name and location of the file containing the roughness data");
        public static readonly ConfigurationSetting InitialWaterLevelFile = new ConfigurationSetting(key: "initialWaterLevelFile", description: "Name and location of the file containing the spatial definition of the initial water level");
        public static readonly ConfigurationSetting InitialWaterDepthFile = new ConfigurationSetting(key: "initialWaterDepthFile", description: "Name and location of the file containing the spatial definition of the initial water depth");
        public static readonly ConfigurationSetting InitialDischargeFile = new ConfigurationSetting(key: "initialDischargeFile", description: "Name and location of the file containing the spatial definition of the initial discharge");
        public static readonly ConfigurationSetting InitialSalinityFile = new ConfigurationSetting(key: "initialSalinityFile", description: "Name and location of the file containing the spatial definition of the initial salinity");
        public static readonly ConfigurationSetting InitialTemperatureFile = new ConfigurationSetting(key: "initialTemperatureFile", description: "Name and location of the file containing the spatial definition of the initial temperature");
        public static readonly ConfigurationSetting DispersionFile = new ConfigurationSetting(key: "dispersionFile", description: "Name and location of the file containing the spatial definition of the dispersion");
        public static readonly ConfigurationSetting DispersionF3File = new ConfigurationSetting(key: "f3File", description: "Name and location of the file containing the spatial definition of the F3 term");
        public static readonly ConfigurationSetting DispersionF4File = new ConfigurationSetting(key: "f4File", description: "Name and location of the file containing the spatial definition of the F4 term");
        public static readonly ConfigurationSetting SalinityParametersFile = new ConfigurationSetting(key: "salinityParametersFile", description: "Name and location of the file containing the salinity parameters");
        public static readonly ConfigurationSetting WindShieldingFile = new ConfigurationSetting(key: "windShieldingFile", description: "Name and location of the file containing the spatial definition of the wind shielding");
        public static readonly ConfigurationSetting BoundaryLocationsFile = new ConfigurationSetting(key: "boundLocFile", description: "Name and location of the file containing the boundary locations");
        public static readonly ConfigurationSetting LateralDischargeLocationsFile = new ConfigurationSetting(key: "latDischargeLocFile", description: "Name and location of the file containing the lateral discharge locations");
        public static readonly ConfigurationSetting BoundaryConditionsFile = new ConfigurationSetting(key: "boundCondFile", description: "Name and location of the file containing the boundary conditions and lateral discharges");
        public static readonly ConfigurationSetting SobekSimIniFile = new ConfigurationSetting(key: "sobekSimIniFile", description: "Name of the sobek sim ini file containing also model parameter which are not in here. Will be phased out later");
        public static readonly ConfigurationSetting RetentionFile = new ConfigurationSetting(key: "retentionFile", description: "Name of the retention ini file containing the values of the retention areas");
        public static readonly ConfigurationSetting LogFile = new ConfigurationSetting(key: "logFile", description: "Name of the log file");

        public const string GlobalValuesHeader = "GlobalValues";
        public static readonly ConfigurationSetting UseInitialWaterDepth = new ConfigurationSetting(key: "UseInitialWaterDepth", description: "Use initial water depth instead of water level. 0=false, 1=true");
        public static readonly ConfigurationSetting InitialWaterLevel = new ConfigurationSetting(key: "InitialWaterLevel", description: "Initial water level for locations where no spatial varying values is defined");
        public static readonly ConfigurationSetting InitialWaterDepth = new ConfigurationSetting(key: "InitialWaterDepth", description: "Initial water depth for locations where no spatial varying values is defined");
        public static readonly ConfigurationSetting InitialDischarge = new ConfigurationSetting(key: "InitialDischarge", description: "Initial discharge for branches where no spatial varying values is defined");
        public static readonly ConfigurationSetting InitialSalinity = new ConfigurationSetting(key: "InitialSalinity", description: "Initial salinity for locations where no spatial varying values is defined");
        public static readonly ConfigurationSetting Dispersion = new ConfigurationSetting(key: "Dispersion", description: "Dispersion for locations where no spatial varying values is defined");
        public static readonly ConfigurationSetting DispersionF3 = new ConfigurationSetting(key: "F3", description: "Dispersion (F3) for locations where no spatial varying values is defined");
        public static readonly ConfigurationSetting DispersionF4 = new ConfigurationSetting(key: "F4", description: "Dispersion (F4) for locations where no spatial varying values is defined");

        public const string InitialConditionsValuesHeader = "InitialConditions";
        public static readonly ConfigurationSetting InitialEmptyWells = new ConfigurationSetting(key: "InitialEmptyWells", description: "0=false, 1=true");

        public const string ResultsNodesHeader = "ResultsNodes";
        public const string ResultsBranchesHeader = "ResultsBranches";
        public const string ResultsStructuresHeader = "ResultsStructures";
        public const string ResultsPumpsHeader = "ResultsPumps";
        public const string ResultsWaterBalanceHeader = "ResultsWaterBalance";
        public const string ResultsObservationsPointsHeader = "ResultsObservationPoints";
        public const string ResultsLateralsHeader = "ResultsLaterals";
        public const string ResultsRetentionsHeader = "ResultsRetentions";

        public const string TimeHeader = "Time";
        public static readonly ConfigurationSetting StartTime = new ConfigurationSetting(key: "StartTime", description: "yyyy-MM-dd HH:mm:ss");
        public static readonly ConfigurationSetting StopTime = new ConfigurationSetting(key: "StopTime", description: "yyyy-MM-dd HH:mm:ss");
        public static readonly ConfigurationSetting TimeStep = new ConfigurationSetting(key: "TimeStep", description: "in seconds");
        public static readonly ConfigurationSetting OutTimeStepGridPoints = new ConfigurationSetting(key: "OutTimeStepGridPoints", description: "in seconds");
        public static readonly ConfigurationSetting OutTimeStepStructures = new ConfigurationSetting(key: "OutTimeStepStructures", description: "in seconds");
        
        public const string ResultsGeneralValuesHeader = "ResultsGeneral";
        public static readonly ConfigurationSetting DelwaqNoStaggeredGrid = new ConfigurationSetting(key: "DelwaqNoStaggeredGrid", description: "0=false, 1=true");
        public static readonly ConfigurationSetting FlowAnalysisTimeSeries = new ConfigurationSetting(key: "FlowAnalysisTimeSeries", description: "0=false, 1=true");
        public static readonly ConfigurationSetting SobeksimStamp = new ConfigurationSetting(key: "SobeksimStamp", description: "0=false, 1=true");

        public const string SedimentValuesHeader = "Sediment";
        public static readonly ConfigurationSetting D50 = new ConfigurationSetting(key: "D50", description: "");
        public static readonly ConfigurationSetting D90 = new ConfigurationSetting(key: "D90", description: "");
        public static readonly ConfigurationSetting DepthUsedForSediment = new ConfigurationSetting(key: "DepthUsedForSediment", description: "");
        
        public const string SpecialsValuesHeader = "Specials";
        public static readonly ConfigurationSetting DesignFactorDLG = new ConfigurationSetting(key: "DesignFactorDLG", description: "");

        public const string NumericalParametersValuesHeader = "NumericalParameters";
        public static readonly ConfigurationSetting AccelerationTermFactor = new ConfigurationSetting(key: "AccelerationTermFactor", description: "");
        public static readonly ConfigurationSetting AccurateVersusSpeed = new ConfigurationSetting(key: "AccurateVersusSpeed", description: "");
        public static readonly ConfigurationSetting CourantNumber = new ConfigurationSetting(key: "CourantNumber", description: "");
        public static readonly ConfigurationSetting DtMinimum = new ConfigurationSetting(key: "DtMinimum", description: "");
        public static readonly ConfigurationSetting EpsilonValueVolume = new ConfigurationSetting(key: "EpsilonValueVolume", description: "");
        public static readonly ConfigurationSetting EpsilonValueWaterDepth = new ConfigurationSetting(key: "EpsilonValueWaterDepth", description: "");
        public static readonly ConfigurationSetting FloodingDividedByDrying = new ConfigurationSetting(key: "FloodingDividedByDrying", description: "");
        public static readonly ConfigurationSetting Gravity = new ConfigurationSetting(key: "Gravity", description: "");
        public static readonly ConfigurationSetting MaxDegree = new ConfigurationSetting(key: "MaxDegree", description: "");
        public static readonly ConfigurationSetting MaxIterations = new ConfigurationSetting(key: "MaxIterations", description: "");
        public static readonly ConfigurationSetting MaxTimeStep = new ConfigurationSetting(key: "MaxTimeStep", description: "");
        public static readonly ConfigurationSetting MinimumSurfaceatStreet = new ConfigurationSetting(key: "MinimumSurfaceatStreet", description: "");
        public static readonly ConfigurationSetting MinimumSurfaceinNode = new ConfigurationSetting(key: "MinimumSurfaceinNode", description: "");
        public static readonly ConfigurationSetting MinimumLength = new ConfigurationSetting(key: "MinimumLength", description: "");
        public static readonly ConfigurationSetting RelaxationFactor = new ConfigurationSetting(key: "RelaxationFactor", description: "");
        public static readonly ConfigurationSetting Rho = new ConfigurationSetting(key: "Rho", description: "");
        public static readonly ConfigurationSetting StructureInertiaDampingFactor = new ConfigurationSetting(key: "StructureInertiaDampingFactor", description: "");
        public static readonly ConfigurationSetting Theta = new ConfigurationSetting(key: "Theta", description: "");
        public static readonly ConfigurationSetting ThresholdValueFlooding = new ConfigurationSetting(key: "ThresholdValueFlooding", description: "");
        public static readonly ConfigurationSetting UseOmp = new ConfigurationSetting(key: "UseOmp", description: "0=false, 1=true");
        public static readonly ConfigurationSetting UseTimeStepReducerStructures = new ConfigurationSetting(key: "UseTimeStepReducerStructures", description: "0=false, 1=true");

        public const string SimulationOptionsValuesHeader = "SimulationOptions";
        public static readonly ConfigurationSetting allowablelargertimestep = new ConfigurationSetting(key: "allowablelargertimestep");
        public static readonly ConfigurationSetting allowabletimesteplimiter = new ConfigurationSetting(key: "allowabletimesteplimiter");
        public static readonly ConfigurationSetting AllowableVolumeError = new ConfigurationSetting(key: "AllowableVolumeError");
        public static readonly ConfigurationSetting AllowCrestLevelBelowBottom = new ConfigurationSetting(key: "AllowCrestLevelBelowBottom");
        public static readonly ConfigurationSetting Cflcheckalllinks = new ConfigurationSetting(key: "Cflcheckalllinks");
        public static readonly ConfigurationSetting Channel = new ConfigurationSetting(key: "Channel");
        public static readonly ConfigurationSetting CheckFuru = new ConfigurationSetting(key: "CheckFuru");
        public static readonly ConfigurationSetting CheckFuruMode = new ConfigurationSetting(key: "CheckFuruMode");
        public static readonly ConfigurationSetting Debug = new ConfigurationSetting(key: "Debug", description: "0=false, 1=true");
        public static readonly ConfigurationSetting DebugTime = new ConfigurationSetting(key: "DebugTime");
        public static readonly ConfigurationSetting DepthsBelowBobs = new ConfigurationSetting(key: "DepthsBelowBobs");
        public static readonly ConfigurationSetting DispMaxFactor = new ConfigurationSetting(key: "DispMaxFactor");
        public static readonly ConfigurationSetting DumpInput = new ConfigurationSetting(key: "DumpInput", description: "0=false, 1=true");
        public static readonly ConfigurationSetting Iadvec1D = new ConfigurationSetting(key: "Iadvec1D");
        public static readonly ConfigurationSetting Jchecknans = new ConfigurationSetting(key: "Jchecknans");
        public static readonly ConfigurationSetting Junctionadvection = new ConfigurationSetting(key: "Junctionadvection");
        public static readonly ConfigurationSetting LaboratoryTest = new ConfigurationSetting(key: "LaboratoryTest");
        public static readonly ConfigurationSetting LaboratoryTimeStep = new ConfigurationSetting(key: "LaboratoryTimeStep");
        public static readonly ConfigurationSetting LaboratoryTotalStep = new ConfigurationSetting(key: "LaboratoryTotalStep ");
        public static readonly ConfigurationSetting Limtyphu1D = new ConfigurationSetting(key: "Limtyphu1D");
        public static readonly ConfigurationSetting LoggingLevel = new ConfigurationSetting(key: "LoggingLevel");
        public static readonly ConfigurationSetting Manhloss = new ConfigurationSetting(key: "Manhloss");
        public static readonly ConfigurationSetting ManholeLosses = new ConfigurationSetting(key: "ManholeLosses");
        public static readonly ConfigurationSetting MissingValue = new ConfigurationSetting(key: "MissingValue");
        public static readonly ConfigurationSetting Momdilution1D = new ConfigurationSetting(key: "Momdilution1D");
        public static readonly ConfigurationSetting Morphology = new ConfigurationSetting(key: "Morphology");
        public static readonly ConfigurationSetting PreissmannMinClosedManholes = new ConfigurationSetting(key: "PreissmannMinClosedManholes");
        public static readonly ConfigurationSetting QDrestart = new ConfigurationSetting(key: "QDrestart");
        public static readonly ConfigurationSetting River = new ConfigurationSetting(key: "River");
        public static readonly ConfigurationSetting Sewer = new ConfigurationSetting(key: "Sewer");
        public static readonly ConfigurationSetting SiphonUpstreamThresholdSwitchOff = new ConfigurationSetting(key: "SiphonUpstreamThresholdSwitchOff");
        public static readonly ConfigurationSetting StrucAlfa = new ConfigurationSetting(key: "StrucAlfa");
        public static readonly ConfigurationSetting StructureDynamicsFactor = new ConfigurationSetting(key: "StructureDynamicsFactor");
        public static readonly ConfigurationSetting StructureStabilityFactor = new ConfigurationSetting(key: "StructureStabilityFactor");
        public static readonly ConfigurationSetting ThresholdForSummerDike = new ConfigurationSetting(key: "ThresholdForSummerDike");
        public static readonly ConfigurationSetting TimersOutputFrequency = new ConfigurationSetting(key: "TimersOutputFrequency");
        public static readonly ConfigurationSetting use1d2dcoupling = new ConfigurationSetting(key: "use1d2dcoupling");
        public static readonly ConfigurationSetting UseEnergyHeadStructures = new ConfigurationSetting(key: "UseEnergyHeadStructures");
        public static readonly ConfigurationSetting UseTimers = new ConfigurationSetting(key: "UseTimers", description: "0=false, 1=true");
        public static readonly ConfigurationSetting Usevariableteta = new ConfigurationSetting(key: "Usevariableteta");
        public static readonly ConfigurationSetting VolumeCheck = new ConfigurationSetting(key: "VolumeCheck");
        public static readonly ConfigurationSetting VolumeCorrection = new ConfigurationSetting(key: "VolumeCorrection");
        public static readonly ConfigurationSetting WaterQualityInUse = new ConfigurationSetting(key: "WaterQualityInUse");
        public static readonly ConfigurationSetting WriteNetCDF = new ConfigurationSetting(key: "WriteNetCDF", description: "0=false, 1=true");
        public static readonly ConfigurationSetting ReadNetworkFromUGrid = new ConfigurationSetting(key: "ReadNetworkFromUGrid", description: "0=false, 1=true");

        public const string TransportComputationValuesHeader = "TransportComputation";
        public static readonly ConfigurationSetting UseTemperature = new ConfigurationSetting(key: "Temperature", description: "0=false, 1=true");
        public static readonly ConfigurationSetting Density = new ConfigurationSetting(key: "Density", description: 
            "Possible values: " +
            "eckart_modified, " +
            "eckart, " +
            "unesco");
        public static readonly ConfigurationSetting HeatTransferModel = new ConfigurationSetting(key: "HeatTransferModel", description:
            "Possible values: " +
            "transport, " +
            "excess, " +
            "composite");
        
        public const string AdvancedOptionsHeader = "AdvancedOptions";
        public static readonly ConfigurationSetting CalculateDelwaqOutput = new ConfigurationSetting(key: "CalculateDelwaqOutput");
        public static readonly ConfigurationSetting ExtraResistanceGeneralStructure = new ConfigurationSetting(key: "ExtraResistanceGeneralStructure");
        public static readonly ConfigurationSetting FillCulvertsWithGL = new ConfigurationSetting(key: "FillCulvertsWithGL", description: "0=false, 1=true");
        public static readonly ConfigurationSetting LateralLocation = new ConfigurationSetting(key: "LateralLocation");
        public static readonly ConfigurationSetting MaxLoweringCrossAtCulvert = new ConfigurationSetting(key: "MaxLoweringCrossAtCulvert");
        public static readonly ConfigurationSetting MaxVolFact = new ConfigurationSetting(key: "MaxVolFact");
        public static readonly ConfigurationSetting NoNegativeQlatWhenThereIsNoWater = new ConfigurationSetting(key: "NoNegativeQlatWhenThereIsNoWater", description: "0=false, 1=true");
        public static readonly ConfigurationSetting TransitionHeightSD = new ConfigurationSetting(key: "TransitionHeightSD");
        public static readonly ConfigurationSetting Latitude = new ConfigurationSetting(key: "Latitude");
        public static readonly ConfigurationSetting Longitude = new ConfigurationSetting(key: "Longitude");

        public const string SalinityValuesHeader = "Salinity";
        public static readonly ConfigurationSetting SaltComputation = new ConfigurationSetting(key: "SaltComputation", description: "0=false, 1=true");
        public static readonly ConfigurationSetting DiffusionAtBoundaries = new ConfigurationSetting(key: "DiffusionAtBoundaries", description: "0=false, 1=true");

        public const string TemperatureValuesHeader = "Temperature";
        public static readonly ConfigurationSetting BackgroundTemperature = new ConfigurationSetting(key: "BackgroundTemperature");
        public static readonly ConfigurationSetting SurfaceArea = new ConfigurationSetting(key: "SurfaceArea", description: "Exposed surface area (used in Excess model)");
        public static readonly ConfigurationSetting AtmosphericPressure = new ConfigurationSetting(key: "AtmosphericPressure", description: "Atmospheric pressure");
        public static readonly ConfigurationSetting DaltonNumber = new ConfigurationSetting(key: "DaltonNumber", description: "Dalton number");
        public static readonly ConfigurationSetting StantonNumber = new ConfigurationSetting(key: "StantonNumber", description: "Stanton number");
        public static readonly ConfigurationSetting HeatCapacity = new ConfigurationSetting(key: "HeatCapacityWater", description: "Heat capacity of water");

        public const string MorphologyValuesHeader = "Morphology";
        public static readonly ConfigurationSetting CalculateMorphology = new ConfigurationSetting(key: "CalculateMorphology", description: "0=false, 1=true");
        public static readonly ConfigurationSetting AdditionalOutput = new ConfigurationSetting(key: "AdditionalOutput", description: "0=false, 1=true");
        public static readonly ConfigurationSetting SedimentInputFile = new ConfigurationSetting(key: "SedimentInputFile", description: "Name of sediment input file");
        public static readonly ConfigurationSetting MorphologyInputFile = new ConfigurationSetting(key: "MorphologyInputFile", description: "Name of morphology description file");

        public const string ObservationsHeader = "Observations";
        public static readonly ConfigurationSetting InterpolationType = new ConfigurationSetting(key: "InterpolationType", description: "Interpolation type (linear or nearest)");

        public const string RestartHeader = "Restart";
        public static readonly ConfigurationSetting RestartStartTime = new ConfigurationSetting(key: "RestartStartTime", description: "yyyy-MM-dd HH:mm:ss");
        public static readonly ConfigurationSetting RestartStopTime = new ConfigurationSetting(key: "RestartStopTime", description: "yyyy-MM-dd HH:mm:ss");
        public static readonly ConfigurationSetting RestartTimeStep = new ConfigurationSetting(key: "RestartTimeStep", description: "in seconds");
        public static readonly ConfigurationSetting UseRestart = new ConfigurationSetting(key: "UseRestart", description: "0=false, 1=true");
        public static readonly ConfigurationSetting WriteRestart = new ConfigurationSetting(key: "WriteRestart", description: "0=false, 1=true");
        
    }
}