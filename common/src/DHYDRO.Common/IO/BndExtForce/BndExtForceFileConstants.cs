namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Provides new style external forcing file constants.
    /// </summary>
    public static class BndExtForceFileConstants
    {
        /// <summary>
        /// The default file version.
        /// </summary>
        public const string DefaultFileVersion = "2.01";

        /// <summary>
        /// The default file type.
        /// </summary>
        public const string DefaultFileType = "extForce";

        /// <summary>
        /// The realtime discharge value.
        /// </summary>
        public const string RealTimeValue = "realtime";

        /// <summary>
        /// Section headers.
        /// </summary>
        public static class Headers
        {
            public const string General = "General";
            public const string Boundary = "Boundary";
            public const string Lateral = "Lateral";
            public const string Meteo = "Meteo";
        }

        /// <summary>
        /// Property keys.
        /// </summary>
        public static class Keys
        {
            public const string BedLevelDepth = "bndBlDepth";
            public const string BranchId = "branchId";
            public const string Chainage = "chainage";
            public const string Discharge = "discharge";
            public const string FileType = "fileType";
            public const string FileVersion = "fileVersion";
            public const string FlowLinkWidth = "bndWidth1D";
            public const string ForcingFile = "forcingFile";
            public const string ForcingFileType = "forcingFileType";
            public const string Id = "id";
            public const string InterpolationMethod = "interpolationMethod";
            public const string LocationFile = "locationFile";
            public const string LocationType = "locationType";
            public const string Name = "name";
            public const string NodeId = "nodeId";
            public const string NumCoordinates = "numCoordinates";
            public const string Operand = "operand";
            public const string ReturnTime = "returnTime";
            public const string TargetMaskInvert = "targetMaskInvert";
            public const string TargetMaskFile = "targetMaskFile";
            public const string TracerDecayTime = "tracerDecayTime";
            public const string TracerFallVelocity = "tracerFallVelocity";
            public const string Quantity = "quantity";
            public const string XCoordinates = "xCoordinates";
            public const string YCoordinates = "yCoordinates";
        }

        /// <summary>
        /// Known quantity names.
        /// </summary>
        public static class Quantities
        {
            // Boundary
            public const string AbsGenBnd = "absgenbnd";
            public const string CriticalOutflowBnd = "criticaloutflowbnd";
            public const string DischargeBnd = "dischargebnd";
            public const string EmbankmentBnd = "1d2dbnd";
            public const string NeumannBnd = "neumannbnd";
            public const string NormalVelocityBnd = "normalvelocitybnd";
            public const string OutflowBnd = "outflowbnd";
            public const string QhBnd = "qhbnd";
            public const string QhuBnd = "qhubnd";
            public const string RiemannBnd = "riemannbnd";
            public const string SalinityBnd = "salinitybnd";
            public const string SedFracBnd = "sedfracbnd";
            public const string SedimentBnd = "sedimentbnd";
            public const string TangentialVelocityBnd = "tangentialvelocitybnd";
            public const string TemperatureBnd = "temperaturebnd";
            public const string TracerBnd = "tracerbnd";
            public const string AdvectionVelocityBnd = "uxuyadvectionvelocitybnd";
            public const string VelocityBnd = "velocitybnd";
            public const string WaterLevelBnd = "waterlevelbnd";
            public const string WaveEnergyBnd = "waveenergybnd";
            public const string WeirOutflowBnd = "weiroutflowbnd";

            // Meteo
            public const string AirDensity = "airdensity";
            public const string AirPressure = "airpressure";
            public const string AirPressureStress = "airpressure_stressx_stressy";
            public const string AirPressureWind = "airpressure_windx_windy";
            public const string AirPressureWindCharnock = "airpressure_windx_windy_charnock";
            public const string AirTemperature = "airtemperature";
            public const string AtmosphericPressure = "atmosphericpressure";
            public const string Charnock = "charnock";
            public const string Cloudiness = "cloudiness";
            public const string DewPoint = "dewpoint";
            public const string DewPointAirTempCloud = "dewpoint_airtemperature_cloudiness";
            public const string DewPointAirTempCloudSolar = "dewpoint_airtemperature_cloudiness_solarradiation";
            public const string Humidity = "humidity";
            public const string HumidityAirTempCloud = "humidity_airtemperature_cloudiness";
            public const string HumidityAirTempCloudSolar = "humidity_airtemperature_cloudiness_solarradiation";
            public const string LongWaveRadiation = "longwaveradiation";
            public const string Rainfall = "rainfall";
            public const string RainfallRate = "rainfall_rate";
            public const string SolarRadiation = "solarradiation";
            public const string SolarRadiationFactor = "solarradiationfactor";
            public const string StressX = "stressx";
            public const string StressXY = "stressxy";
            public const string StressY = "stressy";
            public const string WindSpeedFactor = "windspeedfactor";
            public const string WindStressCoefficient = "windstresscoefficient";
            public const string WindX = "windx";
            public const string WindXY = "windxy";
            public const string WindY = "windy";
        }
    }
}